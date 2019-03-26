using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class BlockedLogs : DBEntity
    {
        public ulong ServerID { get; set; }
        public string BlockedString { get; set; }
    }
}
