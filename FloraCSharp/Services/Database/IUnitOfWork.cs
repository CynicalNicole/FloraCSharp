using FloraCSharp.Services.Database.Repos;
using System;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database
{
    public interface IUnitOfWork : IDisposable
    {
        FloraContext _context { get; }

        IUserRatingRepository UserRatings { get; }
        IReactionsRepository Reactions { get; }
        ICustomRoleRepository CustomRole { get; }
        IBotGamesRepository BotGames { get; }
        IBirthdayRepository Birthdays { get; }

        int Complete();
        Task<int> CompleteAsync();
    }
}
