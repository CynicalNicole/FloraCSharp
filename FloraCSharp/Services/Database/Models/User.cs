using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class User : DBEntity
    {
        public ulong UserID { get; set; }
        public ulong SteamID { get; set; }
    }
}
