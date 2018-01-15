using FloraCSharp.Services.Database.Repos;
using System;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database
{
    public interface IUnitOfWork : IDisposable
    {
        FloraContext _context { get; }

        IUserRatingRepository UserRatings { get; }

        int Complete();
        Task<int> CompleteAsync();
    }
}
