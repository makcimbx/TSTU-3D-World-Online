using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using TSTU.Controller;
using TSTU.Model;

public class PlayerOnlineController : MonoBehaviour
{
    [SerializeField] private Transform playerPrefab;

    private Dictionary<Player, Transform> playerList = new Dictionary<Player, Transform>();

    private Dictionary<Entity, Transform> itemList = new Dictionary<Entity, Transform>();

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
                Dictionary<Player, Transform> toDestroyList = new Dictionary<Player, Transform>();
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
                    Player spawnedPlayer = null;
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


                var listout = new List<Entity>();

                await GameController.Instance.GameServer.UpdateWorldEntityPositionsStream(listout);

                var listin = GameController.Instance.GameServer.OnWorldMapEntityList;
                var deaditem = new Dictionary<Entity, Transform>(); 

                foreach (var item in itemList)
                {
                    var spawnedItem = listin.Find(it => it.eId == item.Key.eId);
                    if (spawnedItem != null)
                    {
                        item.Value.position = new Vector3(spawnedItem.posX, spawnedItem.posY, spawnedItem.posZ);
                    }
                    else
                    {
                        deaditem.Add(item.Key, item.Value);
                    }
                }

                foreach (var item in deaditem)
                {
                    Destroy(item.Value.gameObject);
                    itemList.Remove(item.Key);
                }

                foreach (var item in listin)
                {
                    Entity spawnedEntity = null;
                    foreach (var spawnedenttity in itemList)
                    {
                        if (spawnedenttity.Key.eId == item.eId)
                        {
                            spawnedEntity = spawnedenttity.Key;
                            break;
                        }
                    }

                    if (spawnedEntity == null)
                    {

                        var controller = Instantiate(Inventory.instance.ItemsBase.Find(x => x.typeId == item.itemId).prefab,
                             new Vector3(item.posX, item.posY, item.posZ), Quaternion.identity);
                        var entity = new Entity(item.eId, item.itemId);
                        entity.posX = item.posX;
                        entity.posY = item.posY;
                        entity.posZ = item.posZ;

                        controller.GetComponent<ItemPickup>().item.eId = item.eId;
                        itemList.Add(entity, controller.transform);
                    }
                }

            }
        }
        catch
        {

        }
    }
}
