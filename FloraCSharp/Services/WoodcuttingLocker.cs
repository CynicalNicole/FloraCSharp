using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public class WoodcuttingLocker
    {
        private ConcurrentDictionary<ulong, int> _woodcuttingCooldowns = new ConcurrentDictionary<ulong, int>();

        public void SetWoodcuttingCooldowns(ulong UserID, int set)
        {
            _woodcuttingCooldowns.AddOrUpdate(UserID, set, (i, d) => set);
        }

        public int GetOrCreateUserCooldown(ulong UserID)
        {
            int returnInt = -1;
            if (!_woodcuttingCooldowns.TryGetValue(UserID, out returnInt)) {
                returnInt = -1;
            }

            if (returnInt == -1)
            {
                SetWoodcuttingCooldowns(UserID, 0);
                returnInt = 0;
            }

            return returnInt;
        }
    }
}
