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
        public Transform itemDrag;
        public InventorySlot[] slots;

        public event Action onInventoryChange;

        private List<Item> inventory = new List<Item>();

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


            foreach (var item in slots)
            {                
                item.itemButton.onButtonDrag += item.OnButtonDrag;
                item.onButtonDrag += OnDrag;
                item.itemButton.onButtonDragEnd += item.OnButtonDragEnd;
            }

        }

        bool drag = false;

        void OnDrag(InventorySlot slot)
        {
            if (!drag)
            {
                slot.gameObject.transform.SetParent(itemDrag);
                drag = true;
            }
        }
        
      
        void OnDragEnd(InventorySlot slot)
        {
            slot.gameObject.transform.SetParent(itemsParent);
            drag = false;
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
