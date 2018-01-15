using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class UserRatingRepository : Repository<UserRating>, IUserRatingRepository
    {
        private readonly FloraDebugLogger logger = new FloraDebugLogger();

        public UserRatingRepository(DbContext context) : base(context)
        {
        }

        public UserRating GetUserRating(ulong userID)
        {
            logger.Log("Internal User Rating Debug", "UserRating");
            try
            {
                return _set.FirstOrDefault(x => x.UserID == userID);
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString(), "UserRating");
                return null;
            }
        }

        public void CreateUserRating(ulong userID, int rating)
        {
            _set.Add(new UserRating()
            {
                UserID = userID,
                Rating = rating
            });
            _context.SaveChanges();
        }
    }
}
