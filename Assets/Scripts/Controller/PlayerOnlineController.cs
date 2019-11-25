using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using TSTU.Controller;
using TSTU.Model;
using Dummiesman;
using System.Text;
using System.IO;

public class PlayerOnlineController : MonoBehaviour
{
    [SerializeField] private Transform playerPrefab;

    private Dictionary<Player, Transform> playerList = new Dictionary<Player, Transform>();

    private Dictionary<Entity, Transform> itemList = new Dictionary<Entity, Transform>();

    private KeyValuePair<string, GameObject> playerCustomModel;

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
                Debug.Log("38");
                GameController.Instance.GameServer.CurrentPlayer.PositionOnMap = transform.position;
                await GameController.Instance.GameServer.UpdatePlayerInfoStream();
                Debug.Log("40");
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
                        if (item.playerModel == string.Empty)
                        {
                            var controller = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                            var player = new TSTU.Model.Player(item.Id);
                            player.PositionOnMap = item.PositionOnMap;
                            playerList.Add(player, controller);
                        }
                        else
                        {
                            byte[] byteArray = Encoding.UTF8.GetBytes(item.playerModel);
                            MemoryStream stream = new MemoryStream(byteArray);
                            var prefab = new OBJLoader().Load(stream);
                            var controller = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                            var player = new TSTU.Model.Player(item.Id);
                            player.PositionOnMap = item.PositionOnMap;
                            playerList.Add(player, controller.transform);
                        }
                    }
                }


                var listout = new List<Entity>();
                Debug.Log("99");
                await GameController.Instance.GameServer.UpdateWorldEntityPositionsStream(listout);
                Debug.Log("101");
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

                        var contItem = controller.GetComponent<ItemPickup>();
                        contItem.item = Inventory.СopyItem(entity, contItem.item);

                        itemList.Add(entity, controller.transform);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
