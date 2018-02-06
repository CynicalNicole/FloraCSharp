using Discord;
using Discord.Commands;
using FloraCSharp.Extensions;
using FloraCSharp.Services;
using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Modules
{
    public class Money : ModuleBase
    {
        private FloraDebugLogger _logger;
        private readonly FloraRandom _random;

        public Money(FloraRandom random, FloraDebugLogger logger)
        {
            _random = random;
            _logger = logger;
        }

        [Command("Award"), Summary("Award a user an amount of coin")]
        [FirstOwner]
        public async Task Award(ulong Amount, IUser User)
        {
            Currency cur;
            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Currency.AwardCoins(User.Id, Amount);
                cur = uow.Currency.GetOrCreateCurrency(User.Id);
            }

            await Context.Channel.SendSuccessAsync($"Successfully awarded {User.Username} {Amount}🥕. New Balance: {cur.Coins}🥕");
        }

        [Command("Take"), Summary("Take coins from a user.")]
        [FirstOwner]
        public async Task Take(ulong Amount, IUser User)
        {
            Currency cur;
            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Currency.TakeCoins(User.Id, Amount);
                cur = uow.Currency.GetOrCreateCurrency(User.Id);
            }

            await Context.Channel.SendSuccessAsync($"Successfully taken {Amount}🥕 from {User.Username}. New Balance: {cur.Coins}🥕");
        }

        [Command("Transfer"), Summary("Take coins from a user.")]
        public async Task Transfer(ulong Amount, IUser User)
        {
            //Check if they have enough
            ulong totalCur;
            using (var uow = DBHandler.UnitOfWork())
            {
                totalCur = uow.Currency.GetOrCreateBalance(Context.User.Id);
            }

            if (totalCur < Amount)
            {
                await Context.Channel.SendErrorAsync($"Sorry, you don't have enough FloraCoins. Your balance is {totalCur}🥕");
                return;
            }

            using (var uow = DBHandler.UnitOfWork())
            {
                uow.Currency.TransferCoins(Context.User.Id, User.Id, Amount);
            }

            await Context.Channel.SendSuccessAsync($"{Context.User.Username} has successfully transferred {Amount}🥕 to {User.Username}!");
        }

        [Command("Balance"), Summary("Show user balance.")]
        [Alias("Bal", "$", "Mone", "FloraCoins", "FloCo")]
        public async Task Balance(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            ulong bal;
            using (var uow = DBHandler.UnitOfWork())
            {
                bal = uow.Currency.GetOrCreateBalance(user.Id);
            }

            await Context.Channel.SendSuccessAsync($"{user.Username} has a balance of {bal}🥕");
        }
    }
}
