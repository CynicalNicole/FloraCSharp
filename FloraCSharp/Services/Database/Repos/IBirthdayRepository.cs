using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IBirthdayRepository: IRepository<Birthday>
    {
        Birthday GetBirthday(ulong id);
        List<Birthday> GetAllBirthdays(DateTime date);
        void CreateUserBirthday(ulong id, DateTime date, int age);
    }
}
