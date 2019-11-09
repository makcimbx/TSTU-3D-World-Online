using System.Threading.Tasks;

namespace TSTU.Server
{
    public interface IGameServer
    {
        Task<bool> Login(string login, string password);
    }
}
