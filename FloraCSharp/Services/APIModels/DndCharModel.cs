using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.APIModels
{
    internal class DndCharModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("player")]
        public ulong PlayerID { get; set; }

        [JsonProperty("alignment")]
        public string Alignment { get; set; }

        [JsonProperty("strength")]
        public int Strength { get; set; }

        [JsonProperty("dexterity")]
        public int Dexterity { get; set; }

        [JsonProperty("constitution")]
        public int Constitution { get; set; }

        [JsonProperty("intelligence")]
        public int Intelligence { get; set; }

        [JsonProperty("wisdom")]
        public int Wisdom { get; set; }

        [JsonProperty("Charisma")]
        public int Charisma { get; set; }

        [JsonProperty("profbonus")]
        public int ProficiencyBonus { get; set; }

        [JsonProperty("proficiencies")]
        public List<string> ProficientSkills { get; set; }

        [JsonProperty("doubleProficiencies")]
        public List<string> DoubleProficiencies { get; set; }
    }
}
