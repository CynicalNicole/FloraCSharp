using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public class Cooldowns
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>> _cooldowns = new ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>>();

        private void SetupCommandCooldowns(string CommandName)
        {
            _cooldowns.AddOrUpdate(CommandName, new ConcurrentDictionary<ulong, DateTime>(), (i, d) => new ConcurrentDictionary<ulong, DateTime>());
        }

        public ConcurrentDictionary<ulong, DateTime> GetOrSetupCommandCooldowns(string CommandName)
        {
            ConcurrentDictionary<ulong, DateTime> returnDict = null;
            _cooldowns.TryGetValue(CommandName, out returnDict);

            if (returnDict != null)
                return returnDict;
            else
            {
                SetupCommandCooldowns(CommandName);
                return GetOrSetupCommandCooldowns(CommandName);
            }
        }

        public void UpdateCommandCooldowns(string Command, ConcurrentDictionary<ulong, DateTime> cooldowns)
        {
            _cooldowns.AddOrUpdate(Command, cooldowns, (i, d) => cooldowns);
        }

        public DateTime GetUserCooldownsForCommand(string Command, ulong userID)
        {
            ConcurrentDictionary<ulong, DateTime> cooldowns = null;
            _cooldowns.TryGetValue(Command, out cooldowns);

            if (cooldowns == null)
                return DateTime.MinValue;

            DateTime lastMessage;
            cooldowns.TryGetValue(userID, out lastMessage);

            return lastMessage;
        }

        public void AddUserCooldowns(string Command, ulong userID, DateTime dateTime)
        {
            ConcurrentDictionary<ulong, DateTime> cooldowns = null;
            _cooldowns.TryGetValue(Command, out cooldowns);

            if (cooldowns == null)
                return;

            cooldowns.AddOrUpdate(userID, dateTime, (i, d) => dateTime);
        }
    }
}