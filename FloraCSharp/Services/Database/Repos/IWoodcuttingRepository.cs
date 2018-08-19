using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IWoodcuttingRepository : IRepository<Woodcutting>
    {
        Woodcutting GetOrCreateWoodcutting(ulong id);
        void AddLevel(ulong u, int count = 1);
        void AddXP(ulong u, double xp);
        void AddTree(ulong u, int tree, int count = 1);
        List<Woodcutting> GetTop(int page = 0);
    }
}
