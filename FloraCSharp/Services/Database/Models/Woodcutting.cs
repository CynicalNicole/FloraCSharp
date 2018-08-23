using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class Woodcutting : DBEntity
    {
        public ulong UserID { get; set; }
        public long Gold { get; set; }
        public int Level { get; set; }
        public double XP { get; set; }
        public int NormalTrees { get; set; }
        public int AcheyTrees { get; set; }
        public int OakTrees { get; set; }
        public int WillowTrees { get; set; }
        public int TeakTrees { get; set; }
        public int MapleTrees { get; set; }
        public int HollowTrees { get; set; }
        public int MahoganyTrees { get; set; }
        public int ArcticTrees { get; set; }
        public int YewTrees { get; set; }
        public int SullTrees { get; set; }
        public int MagicTrees { get; set; }
        public int RedwoodTrees { get; set; }
    }
}
