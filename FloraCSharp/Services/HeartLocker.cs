using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public class HeartLocker
    {
        private ulong _lastAttack = 0;
        private int _health = 10;

        public void SetLastAttack(ulong userID)
        {
            _lastAttack = userID;
        }

        public int getHealth()
        {
            return _health;
        }

        public ulong getLastAttack()
        {
            return _lastAttack;
        }

        public void removeHealth()
        {
            _health -= 1;
        }

        public void setHealth(int health)
        {
            _health -= health;
        }
    }
}
