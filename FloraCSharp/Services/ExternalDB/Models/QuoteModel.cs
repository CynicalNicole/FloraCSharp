using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.ExternalDB.Models
{
    public class QuoteModel
    {
        public long ID { get; set; }
        public string Quote { get; set; }
        public string Keyword { get; set; }
    }
}
