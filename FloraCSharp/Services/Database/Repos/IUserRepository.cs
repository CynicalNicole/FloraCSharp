using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IUserRepository : IRepository<User>
    {
        User GetOrCreateUser(ulong UserID, bool Expemption = false);
        int GetBotUserID(ulong UserDiscordID);
        ulong GetSteamID(ulong UserDiscordID);
        string GetDescription(ulong UserDiscordID);
        void setDescription(ulong UserDiscordID, string description);
        void SetSteamID(ulong UserDiscordID, ulong SteamID);
        void SetExemption(ulong UserDiscordID, bool Exemption);
    }
}
