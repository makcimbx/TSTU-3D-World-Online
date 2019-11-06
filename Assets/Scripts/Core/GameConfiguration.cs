using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace TSTU.Core.Configuration
{
    public class GameConfiguration
    {
        private static string configName = "gameConfig";
        private static string configPath = $"{Application.dataPath}/Resources/Configs/{configName}.json";

        public static JSONNode LoadConfig()
        {
            string loadedString = File.ReadAllText(configPath);

            if (loadedString == null)
                return null;

            JSONNode loadedNode = JSON.Parse(loadedString);

            return loadedNode;
        }
    }
}
