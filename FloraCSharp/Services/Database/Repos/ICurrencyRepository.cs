using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        ulong GetOrCreateBalance(ulong UserID);
        Currency GetOrCreateCurrency(ulong UserID);
        void AwardCoins(ulong UserID, ulong amount);
        void TakeCoins(ulong UserID, ulong amount);
        void TransferCoins(ulong UserID, ulong TargetUserID, ulong amount);
        List<Currency> GetTop(int page = 0);
    }
}
