using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using TSTU.Core.Configuration;
using TSTU.Model;
using UnityEngine;

namespace TSTU.Server
{
    public enum ServerState
    {
        Login = 1,
        Registration,
        StartPlayerInfoUpdate,
        PlayerInfoUpdate,
        UpdatePlayerModels,
        AddEntityInInventory,
        DropEntityFromInventory,
        UpdateWorldEntityPositions,
        GetDealerInventory
    }

    public class GameServer : IGameServer, ITickable, IConfigProvider
    {
        private const string configKey = "game_server";
        private const string serverIpAdressKey = "server_ip_adress";
        private const string serverPortKey = "server_port";

        private string serverIpAddress;
        private int serverPort;

        private TSTU.Model.Player player;
        private List<TSTU.Model.Player> otherPlayerList = new List<Model.Player>();
        private List<Entity> onWorldMapEntityList = new List<Entity>();

        public string ConfigKey => configKey;

        public TSTU.Model.Player CurrentPlayer => player;

        public List<TSTU.Model.Player> OtherPlayerList => otherPlayerList;

        public List<Entity> OnWorldMapEntityList => onWorldMapEntityList;

        public void InitializeConfig(JSONNode config)
        {
            serverIpAddress = config[serverIpAdressKey].GetStringOrDefault(string.Empty);
            serverPort = config[serverPortKey].AsInt;
        }

        public void Tick(float dt)
        {

        }

        public async Task<bool> Login(string login, string password, string model)
        {
            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.Login;
            jSONObject["login"] = login;
            jSONObject["password"] = password;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successfull = jsonAnswer["answer"].AsBool;
            var userId = jsonAnswer["user_id"].AsInt;
            if (successfull)
            {
                player = new TSTU.Model.Player(userId);
                player.playerModel = model;
                player.inventory = new Dictionary<int, Entity>();
            }
            return successfull;
        }

        public async Task<bool> Registration(string login, string password)
        {
            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.Registration;
            jSONObject["login"] = login;
            jSONObject["password"] = password;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            return jsonAnswer["answer"].AsBool;
        }

        public async Task<bool> StartPlayerInfoStream()
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.StartPlayerInfoUpdate;
            jSONObject["id"] = player.Id;
            jSONObject["model"] = player.playerModel;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successfull = jsonAnswer["answer"].AsBool;
            if (successfull)
            {
                player.Money = jsonAnswer["money"].AsLong;
            }
            return successfull;
        }

        public async Task<bool> UpdatePlayerInfoStream()
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.PlayerInfoUpdate;
            jSONObject["id"] = player.Id;
            jSONObject["pos_x"] = player.PositionOnMap.x;
            jSONObject["pos_y"] = player.PositionOnMap.y;
            jSONObject["pos_z"] = player.PositionOnMap.z;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successfull = jsonAnswer["answer"].AsBool;
            if (successfull)
            {
                List<TSTU.Model.Player> playerModelUpdateList = new List<Model.Player>();

                var playerMas = jsonAnswer["player_massive"].AsArray;
                List<TSTU.Model.Player> newOtherPlayerList = new List<Model.Player>();
                for (int i = 0; i < playerMas.Count; i++)
                {
                    var otherPlayer = new TSTU.Model.Player(playerMas[i]["id"].AsInt);
                    Vector3 pos = new Vector3(playerMas[i]["pos_x"].AsFloat, playerMas[i]["pos_y"].AsFloat, playerMas[i]["pos_z"].AsFloat);
                    otherPlayer.PositionOnMap = pos;
                    otherPlayer.playerModelMD5Hash = playerMas[i]["model_hash"].GetStringOrDefault(string.Empty);
                    newOtherPlayerList.Add(otherPlayer);

                    var beforePlayer = otherPlayerList.Find(item => item.Id == otherPlayer.Id);
                    if (beforePlayer == null || beforePlayer.playerModelMD5Hash != otherPlayer.playerModelMD5Hash)
                    {
                        playerModelUpdateList.Add(otherPlayer);
                    }
                }

                if (playerModelUpdateList.Count != 0) await UpdatePlayerModelsStream(playerModelUpdateList);

                otherPlayerList = newOtherPlayerList;
            }

            
            return successfull;
        }

        private async Task<bool> UpdatePlayerModelsStream(List<TSTU.Model.Player> players)
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.UpdatePlayerModels;
            for (int i = 0; i < players.Count; i++)
            {
                jSONObject["player_list"][i]["id"] = players[i].Id;
            }
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successful = jsonAnswer["answer"].AsBool;
            if(successful)
            {
                var playerModelsLsit = jsonAnswer["player_list"].AsArray;
                for (int i = 0; i < playerModelsLsit.Count; i++)
                {
                    TSTU.Model.Player playerToUpdate = players.Find(item => item.Id == playerModelsLsit[i]["id"].AsInt);
                    playerToUpdate.playerModel = playerModelsLsit[i]["model"].GetStringOrDefault(string.Empty);
                }
            }
            return successful;
        }

        public async Task<bool> AddEntityInInventoryStream(int placeNumber, int itemId = -1, int eid = -1)
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.AddEntityInInventory;
            jSONObject["id"] = player.Id;
            jSONObject["item_id"] = itemId;
            jSONObject["entity_id"] = eid;
            jSONObject["place_number"] = placeNumber;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successful = jsonAnswer["answer"].AsBool;
            if (successful)
            {
                var inventoryArray = jsonAnswer["inventory"].AsArray;
                player.inventory = new Dictionary<int, Entity>();
                for (int i = 0; i < inventoryArray.Count; i++)
                {
                    int eItemId = inventoryArray[i]["item_id"].AsInt;
                    int eeId = inventoryArray[i]["entity_id"].AsInt;
                    Entity entity = new Entity(eeId, eItemId);
                    player.inventory.Add(inventoryArray[i]["place_number"].AsInt, entity);
                }
                player.Money = jsonAnswer["money"].AsLong;
            }
            return successful;
        }

        public async Task<bool> DropEntityFromInventoryStream(int itemId, int eid, bool isSellToNpc, Vector3 worldDropPos)
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.DropEntityFromInventory;
            jSONObject["id"] = player.Id;
            jSONObject["item_id"] = itemId;
            jSONObject["entity_id"] = eid;
            jSONObject["npc_sell"] = isSellToNpc;
            jSONObject["pos_x"] = worldDropPos.x;
            jSONObject["pos_y"] = worldDropPos.y;
            jSONObject["pos_z"] = worldDropPos.z;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successful = jsonAnswer["answer"].AsBool;
            if (successful)
            {
                var inventoryArray = jsonAnswer["inventory"].AsArray;
                player.inventory = new Dictionary<int, Entity>();
                for (int i = 0; i < inventoryArray.Count; i++)
                {
                    int eItemId = inventoryArray[i]["item_id"].AsInt;
                    int eeId = inventoryArray[i]["entity_id"].AsInt;
                    Entity entity = new Entity(eeId, eItemId);
                    player.inventory.Add(inventoryArray[i]["place_number"].AsInt, entity);
                }
                player.Money = jsonAnswer["money"].AsLong;
            }
            return successful;
        }

        public async Task<bool> UpdateWorldEntityPositionsStream(List<Entity> worldEntitiesToUpdate)
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.UpdateWorldEntityPositions;
            jSONObject["id"] = player.Id;
            for (int i = 0; i < worldEntitiesToUpdate.Count; i++)
            {
                jSONObject["entity_list"][i]["entity_id"] = worldEntitiesToUpdate[i].eId;
                jSONObject["entity_list"][i]["pos_x"] = worldEntitiesToUpdate[i].posX;
                jSONObject["entity_list"][i]["pos_y"] = worldEntitiesToUpdate[i].posY;
                jSONObject["entity_list"][i]["pos_z"] = worldEntitiesToUpdate[i].posZ;
            }
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successful = jsonAnswer["answer"].AsBool;
            if (successful)
            {
                var entitiesArray = jsonAnswer["entity_list"].AsArray;
                onWorldMapEntityList = new List<Entity>();
                for (int i = 0; i < entitiesArray.Count; i++)
                {
                    int eItemId = entitiesArray[i]["item_id"].AsInt;
                    int eeId = entitiesArray[i]["entity_id"].AsInt;
                    Entity entity = new Entity(eeId, eItemId);
                    entity.posX = entitiesArray[i]["pos_x"].AsFloat;
                    entity.posY = entitiesArray[i]["pos_y"].AsFloat;
                    entity.posZ = entitiesArray[i]["pos_z"].AsFloat;
                    onWorldMapEntityList.Add(entity);
                }
            }
            return successful;
        }

        public async Task<Dealer> GetDealerInventory(int dealerId)
        {
            if (player == null) return null;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.GetDealerInventory;
            jSONObject["id"] = player.Id;
            jSONObject["dealer_id"] = dealerId;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            client.Close();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successful = jsonAnswer["answer"].AsBool;
            Dealer dealer = new Dealer(dealerId);
            if (successful)
            {
                var inventoryArray = jsonAnswer["dealer_inventory"].AsArray;
                dealer.inventory = new List<Entity>();
                for (int i = 0; i < inventoryArray.Count; i++)
                {
                    int eItemId = inventoryArray[i]["item_id"].AsInt;
                    int price = inventoryArray[i]["price"].AsInt;
                    Entity entity = new Entity(-1, eItemId);
                    entity.price = price;
                    dealer.inventory.Add(entity);
                }
            }
            return successful ? dealer : null;
        }
    }
}