using System.Collections.Generic;
using System.Threading.Tasks;

namespace TSTU.Server
{
    public interface IGameServer
    {
        Task<bool> Login(string login, string password);
        Task<bool> Registration(string login, string password);
        TSTU.Model.Player CurrentPlayer { get; }
        List<TSTU.Model.Player> OtherPlayerList { get; }
    }
}
