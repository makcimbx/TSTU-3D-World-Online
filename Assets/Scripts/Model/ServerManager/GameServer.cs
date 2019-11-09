using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using TSTU.Core.Configuration;

namespace TSTU.Server
{
    public class GameServer : IGameServer, ITickable, IConfigProvider
    {
        private const string configKey = "game_server";
        private const string serverIpAdressKey = "server_ip_adress";
        private const string serverPortKey = "server_port";

        private string serverIpAddress;
        private int serverPort;

        public string ConfigKey => configKey;

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
    }
}