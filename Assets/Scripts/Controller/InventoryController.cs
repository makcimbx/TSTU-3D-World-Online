using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TSTU.Controller
{
    public class InventoryController : MonoBehaviour
    {
        private GameObject panel;
        public Transform itemsParent;
        public InventorySlot[] slots;

        public event Action onInventoryChange;
        List<Item> inventory = new List<Item>();


        
        
        public void addItem(Item gameObject)
        {
            inventory.Add(gameObject);
            onInventoryChange();
        }

        //public Item GetAndClearItem()
        //{
        //    onInventoryChange();

        //}

        //public Item GetItem()
        //{

        //}

        void Start()
        {
            onInventoryChange += UpdateUI;
            slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        }

        void Update()
        {


        }

        public void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i<inventory.Count)
                {
                    slots[i].AddItem(inventory[i]);
                }
                else
                {
                    slots[i].ClearSlot();
                }
            }
        }

        public void InventoryPanel(GameObject panel)
        {
            this.panel = panel;
            var child = panel.transform.GetChild(panel.transform.childCount - 1);

        }

        internal void SetActive(bool v)
        {
            panel.SetActive(v);
        }
    }
}
