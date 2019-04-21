using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.APIModels
{
    internal class InspirationModel
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("TableNumber")]
        public int TableNumber { get; set; }

        [JsonProperty("CardNumber")]
        public int CardNumber { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}
