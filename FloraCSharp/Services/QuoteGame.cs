using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    class QuoteGame
    {
        public ulong Channel { get; set; }
        public HashSet<QGuess> Guesses { get; set; }
        public string Answer { get; set; }
    }

    public class QGuess
    {
        public ulong UserID { get; set; }
        public string QuoteGuess { get; set; }
        public long Timestamp { get; set; }
    }
}
