using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IUserRatingRepository : IRepository<UserRating>
    {
        UserRating GetUserRating(ulong id);
        void CreateUserRating(ulong id, int rating);
    }
}
