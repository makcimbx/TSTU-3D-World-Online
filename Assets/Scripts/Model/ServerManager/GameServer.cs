using SimpleJSON;
using TSTU.Core.Configuration;

namespace TSTU.Server
{
    public class GameServer : IGameServer, ITickable, IConfigProvider
    {
        private const string configKey = "game_server";
        private const string serverIpAdressKey = "server_ip_adress";
        
        private string serverIpAddress;

        public string ConfigKey => configKey;

        public void InitializeConfig(JSONNode config)
        {
            serverIpAddress = config[serverIpAdressKey].GetStringOrDefault(string.Empty);
        }

        public void Tick(float dt)
        {
            
        }
    }
}