using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IReactionsRepository : IRepository<ReactionModel>
    {
        Task<ReactionModel[]> LoadReactions();
    }
}
