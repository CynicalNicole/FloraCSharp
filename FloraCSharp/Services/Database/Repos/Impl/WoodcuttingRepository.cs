using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class WoodcuttingRepository : Repository<Woodcutting>, IWoodcuttingRepository
    {
        public WoodcuttingRepository(DbContext context) : base(context)
        {
        }

        public void AddLevel(ulong u, int count = 1)
        {
            Woodcutting w = GetOrCreateWoodcutting(u);
            w.Level += count;

            _set.Update(w);
            _context.SaveChanges();
        }

        public void AddTree(ulong u, int tree, int count = 1)
        {
            Woodcutting w = GetOrCreateWoodcutting(u);
           
            switch(tree)
            {
                case 0:
                    w.NormalTrees += count;
                    break;
                case 1:
                    w.AcheyTrees += count;
                    break;
                case 2:
                    w.OakTrees += count;
                    break;
                case 3:
                    w.WillowTrees += count;
                    break;
                case 4:
                    w.TeakTrees += count;
                    break;
                case 5:
                    w.MapleTrees += count;
                    break;
                case 6:
                    w.HollowTrees += count;
                    break;
                case 7:
                    w.MahoganyTrees += count;
                    break;
                case 8:
                    w.ArcticTrees += count;
                    break;
                case 9:
                    w.YewTrees += count;
                    break;
                case 10:
                    w.SullTrees += count;
                    break;
                case 11:
                    w.MagicTrees += count;
                    break;
                case 12:
                    w.RedwoodTrees += count;
                    break;
            }

            _set.Update(w);
            _context.SaveChanges();
        }

        public void AddXP(ulong u, double xp)
        {
            Woodcutting w = GetOrCreateWoodcutting(u);
            w.XP += xp;

            _set.Update(w);
            _context.SaveChanges();
        }

        public Woodcutting GetOrCreateWoodcutting(ulong id)
        {
            Woodcutting toReturn;

            toReturn = _set.FirstOrDefault(x => x.UserID == id);

            if (toReturn == null)
            {
                _set.Add(toReturn = new Woodcutting()
                {
                    UserID = id,
                    Level = 1,
                    XP = 0,
                    NormalTrees = 0,
                    AcheyTrees = 0,
                    OakTrees = 0,
                    WillowTrees = 0,
                    TeakTrees = 0,
                    MapleTrees = 0,
                    HollowTrees = 0,
                    MahoganyTrees = 0,
                    ArcticTrees = 0,
                    YewTrees = 0,
                    SullTrees = 0,
                    MagicTrees = 0,
                    RedwoodTrees = 0
                });
                _context.SaveChanges();
            }

            return toReturn;
        }
    }
}
