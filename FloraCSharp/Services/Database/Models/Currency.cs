using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class Currency : DBEntity
    {
        public ulong UserID { get; set; }
        public ulong Coins { get; set; }
    }
}
