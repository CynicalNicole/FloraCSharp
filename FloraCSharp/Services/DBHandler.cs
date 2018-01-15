using FloraCSharp.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace FloraCSharp.Services
{
    public class DBHandler
    {
        private static DBHandler _instance = null;
        public static DBHandler Instance = _instance ?? (_instance = new DBHandler());
        private readonly DbContextOptions options;
        private readonly FloraDebugLogger logger = new FloraDebugLogger();

        private string connectionString { get; }

        static DBHandler() { }

        private DBHandler()
        {
            connectionString = "Filename=./data/flora.db";
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Filename=./data/flora.db");
            options = optionsBuilder.Options;
        }

        public FloraContext GetFloraContext()
        {
            //logger.Log("Getting Flora Context", "DBHandler");
            var context = new FloraContext(options);
            //logger.Log("Migrating Database", "DBHandler");
            context.Database.Migrate();

            //logger.Log("Returning context", "DBHandler");
            return context;
        }

        private IUnitOfWork GetUnitOfWork() =>
            new UnitOfWork(GetFloraContext());

        public static IUnitOfWork UnitOfWork() =>
            DBHandler.Instance.GetUnitOfWork();
    }
}
