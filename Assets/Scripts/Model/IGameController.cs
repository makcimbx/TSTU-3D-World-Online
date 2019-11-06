using System;
using TSTU.Server;

namespace TSTU.Controller
{
    public interface IGameController
    {
        IGameServer GameServer { get; }
        IObservable<Null> OnSecondPassed { get; }
    }
}