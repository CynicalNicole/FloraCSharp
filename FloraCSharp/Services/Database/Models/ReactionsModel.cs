namespace FloraCSharp.Services.Database.Models
{
    public class ReactionModel : DBEntity
    {
        public string Prompt { get; set; }
        public string Reaction { get; set; }
        public bool AnywhereInSentence { get; set; }
    }
}
