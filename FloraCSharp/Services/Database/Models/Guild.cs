using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class Guild : DBEntity
    {
        public ulong GuildID { get; set; }
        public ulong DeleteLogChannel { get; set; }
        public bool DeleteLogEnabled { get; set; }
    }
}
