using FloraCSharp.Services.Database.Models;
using System.Threading.Tasks;

namespace FloraCSharp.Services.Database.Repos
{
    public interface IBotGamesRepository : IRepository<BotGames>
    {
        Task<BotGames[]> LoadBotGames();
    }
}
