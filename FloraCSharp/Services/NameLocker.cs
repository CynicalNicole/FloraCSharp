using FloraCSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public class NameLocker
    {
        private List<string> cachedNames;
        private DateTime lastCacheTime;

        public void SetCachedNames(List<string> names)
        {
            cachedNames = names;
        }

        public string GetCachedName()
        {
            return cachedNames.RandomItem();
        }

        public DateTime GetLastCacheTime()
        {
            return lastCacheTime;
        }

        public void SetLastCacheTime(DateTime time)
        {
            lastCacheTime = time;
        }
    }
}
