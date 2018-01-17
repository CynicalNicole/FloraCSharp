using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    public class Reactions
    {
        private List<KeyValuePair<string, string>> _reactions = new List<KeyValuePair<string, string>>();
        private readonly FloraRandom _random;

        public Reactions(FloraRandom random)
        {
            _random = random;
        }

        public string GetReactionOrNull(string prompt)
        {
            var possibleReacts = _reactions.Where(x => x.Key.ToLower() == prompt);

            if (possibleReacts.Count() == 0) return null;

            var returnString = possibleReacts.ElementAt(_random.Next(possibleReacts.Count())).Value;
            return returnString;
        }

        public async Task LoadReactionsFromDatabase()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var reactionList = await uow.Reactions.LoadReactions();
                foreach (var reactionVar in reactionList)
                {
                    KeyValuePair<string, string> reaction = new KeyValuePair<string, string>(reactionVar.Prompt, reactionVar.Reaction);
                    _reactions.Add(reaction);
                }
            }
        }

        public async Task HardReload()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var reactionList = await uow.Reactions.LoadReactions();
                List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();
                foreach (var reactionVar in reactionList)
                {
                    KeyValuePair<string, string> reaction = new KeyValuePair<string, string>(reactionVar.Prompt, reactionVar.Reaction);
                    temp.Add(reaction);
                }

                _reactions = new List<KeyValuePair<string, string>>(temp);
            }
        }

        public async Task<int> AddReaction(string prompt, string reaction)
        {
            int reactID = -1;
            using (var uow = DBHandler.UnitOfWork())
            {
                var ReactionMod = new Database.Models.ReactionModel()
                {
                    Prompt = prompt,
                    Reaction = reaction
                };
                uow.Reactions.Add(ReactionMod);
                await uow.CompleteAsync();

                reactID = ReactionMod.ID;
            }

            KeyValuePair<string, string> react = new KeyValuePair<string, string>(prompt, reaction);
            _reactions.Add(react);

            return reactID;
        }

        public async Task RemoveReaction(string prompt)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var allReactions = await uow.Reactions.LoadReactions();
                var reactionsToDelete = allReactions.Where(x => x.Prompt.ToLower() == prompt.ToLower()).ToArray();

                uow.Reactions.RemoveRange(reactionsToDelete);
                await uow.CompleteAsync();
            }

            await HardReload();
        }

        public async Task RemoveReactionByID(int id)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Reactions.Remove(id);
                await uow.CompleteAsync();
            }

            await HardReload();
        }
    }
}
