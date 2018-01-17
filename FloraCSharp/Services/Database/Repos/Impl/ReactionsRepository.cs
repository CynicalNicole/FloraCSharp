using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class ReactionsRepository : Repository<ReactionModel>, IReactionsRepository
    {
        public ReactionsRepository(DbContext context) : base(context)
        {
        }

        public async Task<ReactionModel[]> LoadReactions()
        {
            return await _set.ToArrayAsync();
        }
    }
}
