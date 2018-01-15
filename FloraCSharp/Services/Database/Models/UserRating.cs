namespace FloraCSharp.Services.Database.Models
{
    public class UserRating : DBEntity
    {
        public ulong UserID { get; set; }
        public int Rating { get; set; }
    }
}
