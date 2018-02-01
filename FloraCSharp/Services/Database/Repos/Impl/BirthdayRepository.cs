using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class BirthdayRepository : Repository<Birthday>, IBirthdayRepository
    {
        public BirthdayRepository(DbContext context) : base(context)
        {
        }

        public Birthday GetBirthday(ulong id)
        {
            try
            {
                return _set.FirstOrDefault(x => x.UserID == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Birthday> GetAllBirthdays(DateTime date)
        {
            try
            {
                
                return _set.Where(x => x.Date.Day == date.Day && x.Date.Month == date.Month).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void CreateUserBirthday(ulong id, DateTime date, int age)
        {
            _set.Add(new Birthday()
            {
                UserID = id,
                Date = date,
                Age = age
            });
            _context.SaveChanges();
        }
    }
}
