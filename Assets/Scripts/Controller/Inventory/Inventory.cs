using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TSTU.Model;
using UnityEngine;
namespace TSTU.Controller
{
    public class Inventory : MonoBehaviour
    {
        #region Singletone

        public static Inventory instance;
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("Найдено больше чем один инвентарь");
            }
            instance = this;
        }
        #endregion

        public event Action OnInventoryChange;

        public List<Item> ItemsBase = new List<Item>();

        public bool debug = false;
        
        public Transform CarryItem { get; set; } = null;

        public Dictionary<int, Item> items
        {
            get
            {
                var list = new Dictionary<int, Item>();

                foreach (var item in GameController.Instance.GameServer.CurrentPlayer.inventory)
                {
                    var it = instance.ItemsBase.Find(x => x.typeId == item.Value.itemId);
                    if (it == null)
                    {
                        if(debug)
                            Debug.Log("Несоответствие предметов");

                        continue;
                    }

                    var ret = СopyItem(item.Value, it);

                    list.Add(item.Key, ret);
                }

                return list;
            }
        }

        public static Item GetItem(Entity entity)
        {
            var item = instance.ItemsBase.Find(x => x.typeId == entity.itemId);
            return (item != null) ? СopyItem(entity, item) : null;


        }


        public static Item СopyItem(Entity entity, Item item)
        {
            return new Item()
            {
                eId = entity.eId,
                typeId = item.typeId,
                prefab = item.prefab,
                price = entity.price,
                icon = item.icon,
                isDefaultItem = item.isDefaultItem,
                name = item.name
            };


        }

        public int Count { get => items.Count; }
   

        public int Size = 54;


        public long Money
        {
            get
            {
                Debug.Log("Money" + GameController.Instance.GameServer.CurrentPlayer.Money);
                return GameController.Instance.GameServer.CurrentPlayer.Money;
            }

        }
        public bool isFull { get { return GetFreeSlot() == -1; } }


        public async Task Add(Item item)
        {
            if (!item.isDefaultItem)
            {
                var slot = GetFreeSlot();
                if (slot != -1)
                {
                    await GameController.Instance.GameServer.AddEntityInInventoryStream(slot, item.typeId, item.eId);
                    if (debug)
                        Debug.Log($"Add in {slot} - {item.name}");
                }

              
                OnInventoryChange?.Invoke();
            }
        }

        private int GetFreeSlot()
        {
            var inventory = GameController.Instance.GameServer.CurrentPlayer.inventory;


            for (int i = 0; i < Size; i++)
            {
                if (!inventory.ContainsKey(i))
                {
                    return i;
                }
            }
            return -1;
        }

        public async void Remove(Item item, bool isTrade, Vector3 position)
        {
            await GameController.Instance.GameServer.DropEntityFromInventoryStream(item.typeId, item.eId, isTrade, position);
          
            OnInventoryChange?.Invoke();

        }

        public async void Trade(InventorySlot[] buy, InventorySlot[] sell)
        {
            foreach (var slot in buy)
            {
                if (!slot.isEmpty){
                   
                    await Add(slot.GetAndClearItem());
               
                }
            }


            foreach (var slot in sell)
            {
                if (!slot.isEmpty)
                {
                    var item = slot.GetAndClearItem();
                    Remove(item, true, Vector3.zero);

                }
            }
        }

    }
}