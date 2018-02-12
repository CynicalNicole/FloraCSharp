using FloraCSharp.Services.Database.Repos;
using FloraCSharp.Services.Database.Repos.Impl;
using System;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database
{
    class UnitOfWork : IUnitOfWork
    {
        public FloraContext _context { get; }
        private readonly FloraDebugLogger logger = new FloraDebugLogger();

        private IUserRatingRepository _userRatings;
        public IUserRatingRepository UserRatings => _userRatings ?? (_userRatings = new UserRatingRepository(_context));

        private IReactionsRepository _reactions;
        public IReactionsRepository Reactions => _reactions ?? (_reactions = new ReactionsRepository(_context));

        private ICustomRoleRepository _customRole;
        public ICustomRoleRepository CustomRole => _customRole ?? (_customRole = new CustomRoleRepository(_context));

        private IBotGamesRepository _botGames;
        public IBotGamesRepository BotGames => _botGames ?? (_botGames = new BotGamesRepository(_context));

        private IBirthdayRepository _birthdays;
        public IBirthdayRepository Birthdays => _birthdays ?? (_birthdays = new BirthdayRepository(_context));

        private ICurrencyRepository _currency;
        public ICurrencyRepository Currency => _currency ?? (_currency = new CurrencyRepository(_context));

        private IAttentionRepository _attenion;
        public IAttentionRepository Attention => _attenion ?? (_attenion = new AttentionRepository(_context));

        public UnitOfWork(FloraContext context)
        {
            _context = context;
        }

        public int Complete() =>
            _context.SaveChanges();

        public Task<int> CompleteAsync() =>
            _context.SaveChangesAsync();

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
                if (disposing)
                    _context.Dispose();
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
