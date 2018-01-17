using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FloraCSharp.Services.Database
{
    public class FloraContextFactory : IDbContextFactory<FloraContext>
    {
        private readonly FloraDebugLogger logger = new FloraDebugLogger();
        public FloraContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Filename=data/flora.db");
            return new FloraContext(optionsBuilder.Options);
        }
    }

    public class FloraContext : DbContext
    {
        public DbSet<UserRating> UserRatings { get; set; }
        private readonly FloraDebugLogger logger = new FloraDebugLogger();

        public FloraContext() : base()
        {
        }

        public FloraContext(DbContextOptions options) : base(options)
        {
            //logger.Log("Creating FloraContext with: " + options.Extensions, "FloraContext");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region UserRatings

            var userRatingsEntity = modelBuilder.Entity<UserRating>();
            userRatingsEntity
                .HasIndex(d => d.UserID)
                .IsUnique();

            #endregion

            #region Reactions

            var reactionsEntity = modelBuilder.Entity<ReactionModel>();
            reactionsEntity.HasIndex(d => d.Prompt);

            #endregion

            #region CustomRole

            var customRoleEntity = modelBuilder.Entity<CustomRole>();
            customRoleEntity
                .HasIndex(d => d.UserID)
                .IsUnique();

            #endregion
        }
    }
}
