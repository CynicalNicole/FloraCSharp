using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class Attention : DBEntity
    {
        public ulong UserID { get; set; }
        public int DailyRemaining { get; set; }
        public DateTime LastUsage { get; set; }
        public ulong AttentionPoints { get; set; }
    }
}
