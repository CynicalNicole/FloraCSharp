using FloraCSharp.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database.Repos.Impl
{
    public class BotGamesRepository : Repository<BotGames>, IBotGamesRepository
    {
        public BotGamesRepository(DbContext context) : base(context)
        {
        }

        public int GetDoubleXP()
        {
            throw new System.NotImplementedException();
        }

        public async Task<BotGames[]> LoadBotGames()
        {
            return await _set.ToArrayAsync();
        }

        public void SetDoubleXP(int set)
        {
            throw new System.NotImplementedException();
        }
    }
}
