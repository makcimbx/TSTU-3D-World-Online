using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField]
        private List<Item> Items = new List<Item>();

        public Item[] items { get => Items.ToArray(); }

        public int Count { get => Items.Count; }

        public int Size = 54;


        public int Money = 0;
        public bool isFull { get { return Count == Size; } }

        public void Add(Item item)
        {
            if (!item.isDefaultItem)
            {
                Items.Add(item);
                OnInventoryChange?.Invoke();
            }
        }

        public void Remove(Item item)
        {
            Items.Remove(item);
            OnInventoryChange?.Invoke();

        }


    }
}