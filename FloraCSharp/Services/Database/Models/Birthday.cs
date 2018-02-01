using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class Birthday : DBEntity
    {
        public ulong UserID { get; set; }
        public int Age { get; set; }
        public DateTime Date { get; set; }
    }
}
