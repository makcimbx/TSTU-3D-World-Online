using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using TSTU.Core.Configuration;
using UnityEngine;

namespace TSTU.Server
{
    public enum ServerState
    {
        Login = 1,
        Registration,
        StartPlayerInfoUpdate,
        PlayerInfoUpdate
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

        public string ConfigKey => configKey;

        public TSTU.Model.Player CurrentPlayer => player;

        public List<TSTU.Model.Player> OtherPlayerList => otherPlayerList;

        public void InitializeConfig(JSONNode config)
        {
            serverIpAddress = config[serverIpAdressKey].GetStringOrDefault(string.Empty);
            serverPort = config[serverPortKey].AsInt;
        }

        public void Tick(float dt)
        {

        }

        public async Task<bool> Login(string login, string password)
        {
            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.Login;
            jSONObject["login"] = login;
            jSONObject["password"] = password;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successfull = jsonAnswer["answer"].AsBool;
            var userId = jsonAnswer["user_id"].AsInt;
            if (successfull) player = new TSTU.Model.Player(userId);
            client.Close();
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
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            client.Close();
            return jsonAnswer["answer"].AsBool;
        }

        public async Task<bool> StartPlayerInfoStream()
        {
            if (player == null) return false;

            UdpClient client = new UdpClient();
            JSONObject jSONObject = new JSONObject();
            jSONObject["state"] = (int)ServerState.StartPlayerInfoUpdate;
            jSONObject["id"] = player.Id;
            byte[] data = Encoding.UTF8.GetBytes(jSONObject.ToString());
            client.Send(data, data.Length, serverIpAddress, serverPort);

            var answer = await client.ReceiveAsync();
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            client.Close();
            return jsonAnswer["answer"].AsBool;
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
            string message = Encoding.UTF8.GetString(answer.Buffer);
            JSONNode jsonAnswer = JSON.Parse(message);
            var successfull = jsonAnswer["answer"].AsBool;
            if (successfull)
            {
                var playerMas = jsonAnswer["player_massive"].AsArray;
                otherPlayerList.Clear();
                for (int i = 0; i < playerMas.Count; i++)
                {
                    var otherPlayer = new TSTU.Model.Player(jsonAnswer[i]["id"].AsInt);
                    Vector3 pos = new Vector3(jsonAnswer[i]["pos_x"].AsFloat, jsonAnswer[i]["pos_y"].AsFloat, jsonAnswer[i]["pos_z"].AsFloat);
                    otherPlayerList.Add(otherPlayer);
                }
            }

            client.Close();
            return successfull;
        }
    }
}