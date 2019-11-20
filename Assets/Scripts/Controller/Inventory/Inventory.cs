using System;
using System.Collections;
using System.Collections.Generic;
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

                  
        public Dictionary<int, Item> items
        {
            get
            {
                var list = new Dictionary<int, Item>();

                foreach (var item in GameController.Instance.GameServer.CurrentPlayer.inventory)
                {
                    var it = instance.ItemsBase.Find(x => x.typeId == item.Value.itemId);
                    it.eId = item.Value.eId;
                    list.Add(item.Key, it);
                }

                return list;
            }
        }

        public int Count { get => items.Count; }
   

        public int Size = 54;


        public long Money
        {
            get
            {
                return GameController.Instance.GameServer.CurrentPlayer.Money;
            }

        }
        public bool isFull { get { return GetFreeSlot() == -1; } }


        public async void Add(Item item)
        {
            if (!item.isDefaultItem)
            {
                var slot = GetFreeSlot();
                if (slot != -1)
                    await GameController.Instance.GameServer.AddEntityInInventoryStream(slot, item.typeId, (int)item.eId);
                


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

        public void Remove(Item item)
        {
            OnInventoryChange?.Invoke();

        }

    
    }
}