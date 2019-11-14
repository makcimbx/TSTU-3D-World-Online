using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public Item GetAndClearItem()
        {
           
            Item item = dragItem.GetAndClearItem();
            onInventoryChange();

            return item;
        }

        public Item GetItem()
        {
            Item item = dragItem.GetItem();
            onInventoryChange();

            return item;            
        }

        void Start()
        {
            onInventoryChange += UpdateUI;
            slots = itemsParent.GetComponentsInChildren<InventorySlot>();


            foreach (var item in slots)
            {                
                //item.itemButton.onButtonDrag += item.OnButtonDrag;
                //item.itemButton.onButtonDragEnd += item.OnButtonDragEnd;
                item.onButtonDrag += OnDrag;
                item.onButtonDragEnd += OnDragEnd;
                item.onPointerEnter += OnEnter;
                item.onPointerExit += OnExit;


            }

        }

        public InventorySlot dragItem = null;
        public InventorySlot dragSelected = null;

        public bool Drag = false;


        private void OnDrag(InventorySlot slot)
        {
            if (dragItem == null)
                dragItem = slot;

            if (!Drag)
            {
                dragItem.icon.gameObject.transform.SetParent(itemDrag);
                //OnExit(slot);                
                Drag = true;
            }
            dragItem.icon.transform.position = Input.mousePosition;
            
        }

        private void OnDragEnd(InventorySlot slot)
        {
            dragItem.icon.transform.position = dragItem.transform.position;
            dragItem.icon.gameObject.transform.SetParent(dragItem.itemButton.transform);

            Debug.Log("OnDragEnd");

            if (dragSelected != null)
            {
                dragItem.gameObject.transform.SetSiblingIndex(0);
                //dragSelected.gameObject.transform.GetSiblingIndex()
            }
            dragItem = null;
            Drag = false;
        }

        private void OnEnter(InventorySlot slot)
        {

            dragSelected = slot;


            Debug.Log("OnEnter" + slot.gameObject.transform.GetSiblingIndex());

            //if (dragItem.Equals(slot))
            //    {

            //    }
        }

        private void OnExit(InventorySlot slot)
        {
            if (dragSelected != null)
                dragSelected = null;
            Debug.Log("OnExit" + slot.gameObject.transform.GetSiblingIndex());

        }

        public void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < inventory.Count)
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
