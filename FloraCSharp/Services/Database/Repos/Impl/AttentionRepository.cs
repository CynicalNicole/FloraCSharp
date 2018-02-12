using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class AttentionRepository : Repository<Attention>, IAttentionRepository
    {
        public AttentionRepository(DbContext context) : base(context)
        {
        }

        public Attention GetOrCreateAttention(ulong UserID)
        {
            Attention toReturn;

            toReturn = _set.FirstOrDefault(x => x.UserID == UserID);

            if (toReturn == null)
            {
                _set.Add(toReturn = new Attention()
                {
                    UserID = UserID,
                    LastUsage = DateTime.Now - new TimeSpan(25, 0, 0),
                    DailyRemaining = 3,
                    AttentionPoints = 0
                });
                _context.SaveChanges();
            }

            return toReturn;
        }

        public void AwardAttention(ulong UserID, ulong Amount)
        {
            Attention UserAttention = GetOrCreateAttention(UserID);
            UserAttention.AttentionPoints += Amount;

            _set.Update(UserAttention);
            _context.SaveChanges();
        }

        public List<Attention> GetTop(int page = 0)
        {
            int offset = page * 9;
            return _set.OrderByDescending(x => x.AttentionPoints).Skip(offset).Take(9).ToList();
        }
    }
}
