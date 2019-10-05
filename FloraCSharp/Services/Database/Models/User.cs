using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class User : DBEntity
    {
        public ulong UserID { get; set; }
        public ulong SteamID { get; set; }
        public bool IsExempt { get; set; }
        public string Description { get; set }
    }
}
