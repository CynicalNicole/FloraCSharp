using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }

        public int GetBotUserID(ulong UserDiscordID)
        {
            User u = GetOrCreateUser(UserDiscordID);
            return u.ID;
        }

        public User GetOrCreateUser(ulong UserID)
        {
            User toReturn;
            toReturn = _set.FirstOrDefault(x => x.UserID == UserID);

            if (toReturn == null)
            {
                _set.Add(toReturn = new User()
                {
                    UserID = UserID,
                    SteamID = 0
                });
                _context.SaveChanges();
            }

            return toReturn;
        }

        public ulong GetSteamID(ulong UserDiscordID)
        {
            User u = GetOrCreateUser(UserDiscordID);
            return u.SteamID;
        }

        public void SetSteamID(ulong UserDiscordID, ulong SteamID)
        {
            User u = GetOrCreateUser(UserDiscordID);
            u.SteamID = SteamID;

            _set.Update(u);
            _context.SaveChanges();
        }
    }
}
