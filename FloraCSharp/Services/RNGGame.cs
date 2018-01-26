using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public class RNGGame
    {
        public ulong Channel { get; set; }
        public int MinGuess { get; set; }
        public int MaxGuess { get; set; }
        public HashSet<Guess> Guesses { get; set; }
    }

    public class Guess
    {
        public ulong UserID { get; set; }
        public int GuessIndex { get; set; }
    }
}
