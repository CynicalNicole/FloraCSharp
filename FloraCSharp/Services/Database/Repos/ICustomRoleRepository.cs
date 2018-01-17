using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface ICustomRoleRepository : IRepository<CustomRole>
    {
        CustomRole GetCustomRole(ulong id);
        void CreateCustomRole(ulong userID, ulong roleID);
        void DeleteCustomRole(ulong userID);
    }
}
