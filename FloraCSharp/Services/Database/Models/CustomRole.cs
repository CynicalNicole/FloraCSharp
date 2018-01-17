namespace FloraCSharp.Services.Database.Models
{
    public class CustomRole : DBEntity
    {
        public ulong UserID { get; set; }
        public ulong RoleID { get; set; }
    }
}
