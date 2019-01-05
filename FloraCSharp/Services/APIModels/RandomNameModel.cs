using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.APIModels
{
    internal class RandomNameModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
