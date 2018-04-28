using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp
{
    public class Configuration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public ulong[] Owners { get; set; }
        public string Version { get; set; }
        public bool RotatingGames { get; set; }
        public ulong BirthdayChannel { get; set; }
        public bool Shutdown { get; set; }
        public string SteamAPIKey { get; set; }
    }
}
