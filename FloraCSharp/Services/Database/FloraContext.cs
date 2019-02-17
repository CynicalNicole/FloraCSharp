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
        public DbSet<User> User { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<UserRate> UserRate { get; set; }
        public DbSet<ReactionModel> ReactionModel { get; set; }
        public DbSet<CustomRole> CustomRole { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<BotGames> BotGames { get; set; }
        public DbSet<Birthday> Birthday { get; set; }
        public DbSet<Attention> Attention { get; set; }
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

            #region Channels
            var channelsEntity = modelBuilder.Entity<Channels>();
            channelsEntity
                .HasIndex(d => d.ChannelID)
                .IsUnique();
            #endregion

            #region UserRate
            var userRateEntity = modelBuilder.Entity<UserRate>();
            userRateEntity
                .HasIndex(d => d.ID)
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

            #region BotGames

            var botGamesEntity = modelBuilder.Entity<BotGames>();
            botGamesEntity
                .HasIndex(d => d.GameName)
                .IsUnique();

            #endregion

            #region Birthdays

            var birthdaysEntity = modelBuilder.Entity<Birthday>();
            birthdaysEntity
                .HasIndex(d => d.UserID)
                .IsUnique();

            #endregion

            #region Currency

            var currencyEntity = modelBuilder.Entity<Currency>();
            birthdaysEntity
                .HasIndex(d => d.UserID)
                .IsUnique();

            #endregion

            #region Attention

            var attentionEntity = modelBuilder.Entity<Attention>();
            birthdaysEntity
                .HasIndex(d => d.UserID)
                .IsUnique();

            #endregion

            #region User
            var userEntity = modelBuilder.Entity<User>();
            userEntity
                .HasIndex(d => d.UserID)
                .IsUnique();
            #endregion

            #region Woodcutting
            var woodcuttingEntity = modelBuilder.Entity<Woodcutting>();
            woodcuttingEntity
                .HasIndex(d => d.UserID)
                .IsUnique();
            #endregion
        }
    }
}