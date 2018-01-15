using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services
{
    public interface IBotCredentials
    {
        ulong ClientID { get; }

        string Token { get; }
        bool IsOwner(IUser u);
    }
}
