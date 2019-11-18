using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using TSTU.Controller;

public class PlayerOnlineController : MonoBehaviour
{
    [SerializeField] private Transform playerPrefab;

    private Dictionary<TSTU.Model.Player, Transform> playerList = new Dictionary<TSTU.Model.Player, Transform>();

    private async void Start()
    {
        await GameController.Instance.GameServer.StartPlayerInfoStream();

        UpdateInfo();
        //GameController.Instance.OnSecondPassed.Subscribe(_ => UpdateInfo()).AddTo(this);
    }

    private async void UpdateInfo()
    {
        try
        {
            while (true)
            {
                GameController.Instance.GameServer.CurrentPlayer.PositionOnMap = transform.position;
                await GameController.Instance.GameServer.UpdatePlayerInfoStream();

                var otherPlayerList = GameController.Instance.GameServer.OtherPlayerList;
                Dictionary<TSTU.Model.Player, Transform> toDestroyList = new Dictionary<TSTU.Model.Player, Transform>();
                foreach (var item in playerList)
                {
                    var spawnedPlayer = otherPlayerList.Find(player => player.Id == item.Key.Id);
                    if (spawnedPlayer != null)
                    {
                        item.Value.position = spawnedPlayer.PositionOnMap;
                    }
                    else
                    {
                        toDestroyList.Add(item.Key, item.Value);
                    }
                }

                foreach (var item in toDestroyList)
                {
                    Destroy(item.Value.gameObject);
                    playerList.Remove(item.Key);
                }

                foreach (var item in otherPlayerList)
                {
                    TSTU.Model.Player spawnedPlayer = null;
                    foreach (var spawnedplayers in playerList)
                    {
                        if (spawnedplayers.Key.Id == item.Id)
                        {
                            spawnedPlayer = spawnedplayers.Key;
                            break;
                        }
                    }

                    if (spawnedPlayer == null)
                    {
                        var controller = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                        var player = new TSTU.Model.Player(item.Id);
                        player.PositionOnMap = item.PositionOnMap;
                        playerList.Add(player, controller);
                    }
                }
            }
        }
        catch
        {

        }
    }
}
