using Discord;
using Discord.Commands;
using FloraCSharp.Modules.Games.Common;
using FloraCSharp.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FloraCSharp.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using FloraCSharp.Services.Database.Models;

namespace FloraCSharp.Modules.Games
{
    public class Games : ModuleBase
    {
        private readonly FloraRandom _random;
        private FloraDebugLogger _logger;
        private readonly BotGameHandler _botGames;
        private Services.RNGService _rngservice = new Services.RNGService();

        private List<ulong> OngoingChops = new List<ulong>();

        public Games(FloraRandom random, FloraDebugLogger logger, BotGameHandler botGames)
        {
            _random = random;
            _logger = logger;
            _botGames = botGames;
        }

        private readonly Dictionary<string, int> TreeID = new Dictionary<string, int>()
        {
            { "normal", 0 },
            { "achey", 1 },
            { "oak", 2 },
            { "willow", 3 },
            { "teak", 4 },
            { "maple", 5 },
            { "hollow", 6 },
            { "mahogany", 7 },
            { "arctic_pine", 8},
            { "yew", 9},
            { "sulliuscep", 10},
            { "magic", 11 },
            { "redwood", 12 }
        };

        private enum Axes { iron, steel, black, mithril, adamant, rune, dragon };

        private readonly Dictionary<int, int> TreeLevel = new Dictionary<int, int>
        {
            { 0, 1 },
            { 1, 1 },
            { 2, 15 },
            { 3, 30 },
            { 4, 35 },
            { 5, 45 },
            { 6, 45 },
            { 7, 50 },
            { 8, 54 },
            { 9, 60 },
            { 10, 65 },
            { 11, 75 },
            { 12, 90 }
        };

        private readonly Dictionary<int, double> TreeXP = new Dictionary<int, double>
        {
            { 0, 25 },
            { 1, 25 },
            { 2, 37.5 },
            { 3, 67.5 },
            { 4, 85 },
            { 5, 100 },
            { 6, 82.5 },
            { 7, 125 },
            { 8, 40 },
            { 9, 175 },
            { 10, 127 },
            { 11, 250 },
            { 12, 380 }
        };

        private readonly Dictionary<int, string> AxeLevel = new Dictionary<int, string>
        {
            { 1, "iron" },
            { 6, "steel" },
            { 11, "black" },
            { 21, "mithril" },
            { 31, "adamant" },
            { 41, "rune" },
            { 61, "dragon" }
        };

        private readonly Dictionary<int, double> AxeTiming = new Dictionary<int, double>
        {
            { 0, 14 },
            { 1, 12 },
            { 2, 10 },
            { 3, 8 },
            { 4, 6 },
            { 5, 4 },
            { 6, 3 }
        };

        [Command("RNGGame"), Summary("Starts an RNG Game between given bounds. Optionally specifying a timeout in seconds")]
        [RequireContext(ContextType.Guild)]
        public async Task RNGGame([Summary("The minimum, inclusive bound")] int min, [Summary("The maximum, exclusive bound")] int max, int timeout = 30)
        {
            if (timeout > 300)
                timeout = 0;

            if (timeout > 0)
            {
                timeout = timeout * 1000;
            }
            else
            {
                timeout = 30000;
            }

            RNGGame Game = new RNGGame
            {
                Channel = Context.Channel.Id,
                MinGuess = min,
                MaxGuess = max,
                Guesses = new HashSet<Guess>()
            };

            if (_rngservice.StartRNGG(Game, (DiscordSocketClient)Context.Client))
            {
                await Context.Channel.SendSuccessAsync($"RNG Game (Min: {min}, Max: {max})", $"Game started! Type your guesses now! You have {timeout/1000} seconds.");
                await Task.Delay(timeout);
                await _rngservice.EndGameInChannel(Context.Guild, Context.Channel, _random);
            }
        }

        [Command("Chop"), Summary("Chops 29 of a specified tree type with your best equipped axe.")]
        [RequireContext(ContextType.Guild)]
        public async Task Chop(int chopcount, [Summary("The tree type"), Remainder] string tree)
        {
            if (OngoingChops.Contains(Context.User.Id))
            {
                await Context.Channel.SendErrorAsync("Woodcutting", $"{Context.User.Username}, you already are chopping trees.");
            }

            if (chopcount < 1) chopcount = 1;
            if (chopcount > 28) chopcount = 28;

            //Get tree ID
                string treeString = tree.ToLower().Replace(' ', '_');
            int tID;
            
            if (!TreeID.TryGetValue(treeString, out tID))
            {
                await Context.Channel.SendErrorAsync("Woodcutting", "Tree does not exist");
                return;
            }
            _logger.Log("Woodcutting", $"Tree ID: {tID}");

            //Okay let's get the User
            Woodcutting wc;

            using (var uow = DBHandler.UnitOfWork())
            {
                wc = uow.Woodcutting.GetOrCreateWoodcutting(Context.User.Id);
            }

            //Check if they can even chop this tree
            int treeLevel = TreeLevel[tID];

            if (wc.Level < treeLevel)
            {
                await Context.Channel.SendErrorAsync("Woodcutting", $"You need {treeLevel} woodcutting to chop down {tree} trees.");
                return;
            }

            //Cool they can now
            //What's their axe level
            string axetype = "";
            foreach (int key in AxeLevel.Keys)
            {
                if (axetype != "")
                    break;
                else if (wc.Level >= key)
                    axetype = AxeLevel[key];
            }

            _logger.Log("Woodcutting", $"Axe Type: {axetype}");

            //Soemthing gone wrong?
            if (axetype == "")
            {
                await Context.Channel.SendErrorAsync("Big error oh no.");
                return;
            }

            //Okay we know their axetype, get axeID
            int aID = -1;
            switch (axetype)
            {
                case "iron":
                    aID = (int) Axes.iron;
                    break;
                case "steel":
                    aID = (int)Axes.steel;
                    break;
                case "black":
                    aID = (int)Axes.black;
                    break;
                case "mithril":
                    aID = (int)Axes.mithril;
                    break;
                case "adamant":
                    aID = (int)Axes.adamant;
                    break;
                case "rune":
                    aID = (int)Axes.rune;
                    break;
                case "dragon":
                    aID = (int)Axes.dragon;
                    break;
            }

            //We have the tree and axeID
            //Now we have to get the needed wait, and tree XP
            double tXP = TreeXP[tID] * chopcount;
            _logger.Log("Woodcutting", $"XP: {tXP}");
            double tWait = AxeTiming[aID] * chopcount;
            _logger.Log("Woodcutting", $"Wait: {tWait}s");

            //Add them to the list
            OngoingChops.Add(Context.User.Id);

            //Okay lets begin
            //First we w a i t
            await Context.Channel.SendSuccessAsync("Woodcutting", $"You swing your {axetype} axe at the {tree} tree, {Context.User.Username}.\n This will take: {tWait} seconds.");
            await Task.Delay((int) (tWait * 1000));

            bool levelUpFlag = false;

            //Add xp, add tree type
            using (var uow = DBHandler.UnitOfWork())
            {
                //So lets check the next level.
                int nextLevel = wc.Level + 1;

                //Next xp
                long nextXP = CalculateNextLevelEXP(nextLevel);

                double newXP = wc.XP + tXP;

                //Do the mathsy shit
                uow.Woodcutting.AddXP(wc.UserID, tXP);

                bool levelUp = true;

                while (levelUp)
                {
                    //Is this a l e v e l u p ? 
                    if (newXP < nextXP) levelUp = false;

                    if (levelUp)
                    {
                        levelUpFlag = true;
                        nextLevel += 1;
                        nextXP = CalculateNextLevelEXP(nextLevel);
                        uow.Woodcutting.AddLevel(wc.UserID);
                    }  
                }

                _logger.Log("Woodcutting", $"New Level: {nextLevel - 1}");

                //Work 
                uow.Woodcutting.AddTree(wc.UserID, tID, chopcount);

                wc = uow.Woodcutting.GetOrCreateWoodcutting(Context.User.Id);
            }

            //F iiiinally
            OngoingChops.Remove(Context.User.Id);
            if (levelUpFlag) await Context.Channel.SendMessageAsync($"{Context.User.Mention} has levelled up to {wc.Level} woodcutting!");
            await Context.Channel.SendSuccessAsync("Woodcutting", $"After {tWait * chopcount} seconds you chop down {chopcount} {tree} tree(s), {Context.User.Username}.\n Level: {wc.Level} | XP: {wc.XP}");
        }

        private static long CalculateNextLevelEXP(int nextLevel)
        {
            if (nextLevel == 1) return 0;

            double sum = 0;
            for (double i = 1; i < nextLevel; i++)
            {
                double power = Math.Pow(2, (double)i / 7);
                sum += Math.Floor(i + (300 * power));
            }

            return (long)Math.Floor(sum * 0.25);
        }
    }
}
