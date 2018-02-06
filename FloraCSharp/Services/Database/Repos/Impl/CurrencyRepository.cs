using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(DbContext context) : base(context)
        {
        }

        public ulong GetOrCreateBalance(ulong UserID)
        {
            return _set.FirstOrDefault(x => x.UserID == UserID)?.Coins ?? CreateAndReturnBalance(UserID);
        }

        public Currency GetOrCreateCurrency(ulong UserID)
        {
            Currency toReturn;

            toReturn = _set.FirstOrDefault(x => x.UserID == UserID);

            if (toReturn == null)
            {
                _set.Add(toReturn = new Currency()
                {
                    UserID = UserID,
                    Coins = 0
                });
                _context.SaveChanges();
            }

            return toReturn;
        }

        private ulong CreateAndReturnBalance(ulong UserID)
        {
            Currency cur = new Currency()
            {
                UserID = UserID,
                Coins = 0
            };
            _set.Add(cur);
            _context.SaveChanges();
            return cur.Coins;
        }

        public void AwardCoins(ulong UserID, ulong Amount)
        {
            Currency UserCurrency = GetOrCreateCurrency(UserID);
            UserCurrency.Coins += Amount;

            _set.Update(UserCurrency);
            _context.SaveChanges();
        }

        public void TakeCoins(ulong UserID, ulong Amount)
        {
            Currency UserCurrency = GetOrCreateCurrency(UserID);
            UserCurrency.Coins -= Amount;

            _set.Update(UserCurrency);
            _context.SaveChanges();
        }
        
        public void TransferCoins(ulong UserID, ulong TargetUserID, ulong Amount)
        {
            TakeCoins(UserID, Amount);
            AwardCoins(TargetUserID, Amount);
        }
    }
}
