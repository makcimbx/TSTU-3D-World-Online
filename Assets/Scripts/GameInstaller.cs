using Zenject;
using TSTU.Server;

namespace TSTU.Installer
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(IGameServer), typeof(ITickable)).To<GameServer>().AsCached().NonLazy();
        }
    }
}