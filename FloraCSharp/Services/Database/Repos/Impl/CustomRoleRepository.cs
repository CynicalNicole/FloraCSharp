using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class CustomRoleRepository : Repository<CustomRole>, ICustomRoleRepository
    {
        public CustomRoleRepository(DbContext context) : base(context)
        {
        }

        public CustomRole GetCustomRole(ulong id)
        {
            try
            {
                return _set.FirstOrDefault(x => x.UserID == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void CreateCustomRole(ulong userID, ulong roleID)
        {
            _set.Add(new CustomRole()
            {
                UserID = userID,
                RoleID = roleID
            });
            _context.SaveChanges();
        }

        public void DeleteCustomRole(ulong userID)
        {
            CustomRole CR = _set.FirstOrDefault(x => x.RoleID == userID);
            _set.Remove(CR);
            _context.SaveChanges();
        }
    }
}
