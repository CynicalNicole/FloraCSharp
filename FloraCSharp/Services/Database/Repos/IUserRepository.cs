using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IUserRepository : IRepository<User>
    {
        User GetOrCreateUser(ulong UserID);
        int GetBotUserID(ulong UserDiscordID);
        ulong GetSteamID(ulong UserDiscordID);
        void SetSteamID(ulong UserDiscordID, ulong SteamID);
    }
}
