using System.Collections.Generic;
using System.Threading.Tasks;
using TSTU.Model;
using UnityEngine;

namespace TSTU.Server
{
    public interface IGameServer
    {
        Task<bool> Login(string login, string password, string model);
        Task<bool> Registration(string login, string password);
        Task<bool> StartPlayerInfoStream();
        Task<bool> UpdatePlayerInfoStream();
        Task<bool> AddEntityInInventoryStream(int placeNumber, int itemId = -1, long eid = -1);
        Task<bool> DropEntityFromInventoryStream(int itemId, long eid, bool isSellToNpc, Vector3 worldDropPos);
        Task<bool> UpdateWorldEntityPositionsStream(List<Entity> worldEntitiesToUpdate);
        Task<Dealer> GetDealerInventory(int dealerId);
        Player CurrentPlayer { get; }
        List<Player> OtherPlayerList { get; }
        List<Entity> OnWorldMapEntityList { get; }
    }
}
