using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using TSTU.Core.Configuration;
using TSTU.Server;
using UniRx;

namespace TSTU.Controller
{
    public class GameController : IGameController
    {
        private static GameController instance = new GameController();

        private List<ITickable> listOfTickableManagers;
        private List<IConfigProvider> listOfWithConfigManagers;

        private GameServer gameServer;

        private Subject<Null> onSecondPassed = new Subject<Null>();

        private float timerTick;

        public IGameServer GameServer => gameServer;
        public IObservable<Null> OnSecondPassed => onSecondPassed;

        public static GameController Instance => instance;

        private GameController()
        {
            gameServer = new GameServer();

            Initialize();
        }

        private void Initialize()
        {
            listOfTickableManagers = GetListOfTickableManagers();
            listOfWithConfigManagers = GetListOfWithConfigManagers();

            JSONNode configData = GameConfiguration.LoadConfig();
            foreach (var item in listOfWithConfigManagers)
            {
                item.InitializeConfig(configData[item.ConfigKey]);
            }
        }

        public void Tick(float dt)
        {
            if (listOfTickableManagers != null)
            {
                foreach (var manager in listOfTickableManagers)
                {
                    manager.Tick(dt);
                }
            }

            timerTick -= dt;
            if (timerTick <= 0)
            {
                timerTick = 1;
                onSecondPassed.OnNext(null);
            }
        }

        private List<ITickable> GetListOfTickableManagers()
        {
            List<ITickable> tickableManagers = new List<ITickable>();
            if (gameServer is ITickable) tickableManagers.Add(gameServer as ITickable);
            return tickableManagers;
        }

        private List<IConfigProvider> GetListOfWithConfigManagers()
        {
            List<IConfigProvider> configManagers = new List<IConfigProvider>();
            if (gameServer is IConfigProvider) configManagers.Add(gameServer as IConfigProvider);
            return configManagers;
        }
    }
}