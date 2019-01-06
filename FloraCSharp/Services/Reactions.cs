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
        struct Reaction
        {
            public string Prompt;
            public string React;
            public bool Anywhere;

            public Reaction(string p, string r, bool a)
            {
                Prompt = p;
                React = r;
                Anywhere = a;
            }
        }

        private List<Reaction> _reactions = new List<Reaction>();
        private readonly FloraRandom _random;

        public Reactions(FloraRandom random)
        {
            _random = random;
        }     

        public string GetReactionOrNull(string prompt)
        {
            var possibleReacts = _reactions.Where(x => x.Prompt.ToLower() == prompt);

            //If there's no direct react we can check if it contains & AnywhereInSentence is true
            if (possibleReacts.Count() == 0)
            {
                possibleReacts = _reactions.Where(x => prompt.Contains(x.Prompt.ToLower()) && x.Anywhere);
            }

            //If there's still noting return null
            if (possibleReacts.Count() == 0) return null;

            var returnString = possibleReacts.ElementAt(_random.Next(possibleReacts.Count())).React;
            return returnString;
        }

        public async Task LoadReactionsFromDatabase()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var reactionList = await uow.Reactions.LoadReactions();
                foreach (var reactionVar in reactionList)
                {
                    Reaction reaction = new Reaction(reactionVar.Prompt, reactionVar.Reaction, reactionVar.AnywhereInSentence);
                    _reactions.Add(reaction);
                }
            }
        }

        public async Task HardReload()
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var reactionList = await uow.Reactions.LoadReactions();
                List<Reaction> temp = new List<Reaction>();
                foreach (var reactionVar in reactionList)
                {
                    Reaction reaction = new Reaction(reactionVar.Prompt, reactionVar.Reaction, reactionVar.AnywhereInSentence);
                    temp.Add(reaction);
                }

                _reactions = new List<Reaction>(temp);
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
                    Reaction = reaction,
                    AnywhereInSentence = false
                };
                uow.Reactions.Add(ReactionMod);
                await uow.CompleteAsync();

                reactID = ReactionMod.ID;
            }

            Reaction react = new Reaction(prompt, reaction, false);
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

        public async Task<bool> SetReactionActivationMethod(int reactionID, bool anywhere)
        {
            using (var uow = DBHandler.UnitOfWork())
            {
                var allReactions = await uow.Reactions.LoadReactions();
                var reactionToEdit = allReactions.Where(x => x.ID == reactionID).FirstOrDefault();

                if (reactionToEdit == null) return false;

                //Edit
                reactionToEdit.AnywhereInSentence = anywhere;
                uow.Reactions.Update(reactionToEdit);
                await uow.CompleteAsync();
            }

            await HardReload();
            return true;
        }
    }
}
