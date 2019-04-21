using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Models
{
    public class DndInspiration : DBEntity
    {
        public int CombinedNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int CardNumber { get; set; }
    }
}
