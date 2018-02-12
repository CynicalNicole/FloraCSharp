using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IAttentionRepository : IRepository<Attention>
    {
        Attention GetOrCreateAttention (ulong UserID);
        void AwardAttention(ulong UserID, ulong Amount);
        List<Attention> GetTop(int Page);
    }
}
