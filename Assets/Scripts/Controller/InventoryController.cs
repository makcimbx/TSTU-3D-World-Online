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
           
            Item item = selectedItem.GetAndClearItem();
            inventory.Remove(item);
            onInventoryChange();

            return item;
        }

        public Item GetItem()
        {
            Item item = selectedItem.GetItem();
            onInventoryChange();

            return item;            
        }

        void Start()
        {
            onInventoryChange += UpdateUI;
            slots = itemsParent.GetComponentsInChildren<InventorySlot>();


            foreach (var item in slots)
            {                            
                item.onButtonDrag += OnDrag;
                item.onButtonDragEnd += OnDragEnd;
                item.onPointerEnter += OnEnter;
                item.onPointerExit += OnExit;
            }

        }

        public InventorySlot dragItem = null;
        public InventorySlot selectedItem = null;
        public bool Drag = false;

        private void OnDrag(InventorySlot slot)
        {
            if (dragItem == null)
                dragItem = slot;

            if (!Drag)
            {
                var color = dragItem.icon.color;
                color.a = 0.3f;
                dragItem.icon.color = color;

                Cursor.SetCursor((Texture2D)dragItem.icon.sprite.texture, Vector2.zero, CursorMode.ForceSoftware);               

                Drag = true;
            }
            //dragItem.icon.transform.position = Input.mousePosition;
            
        }

        private void OnDragEnd(InventorySlot slot)
        {
            dragItem.icon.transform.position = dragItem.transform.position;
           // dragItem.icon.gameObject.transform.SetParent(dragItem.icon.transform);
                    

            if (selectedItem != null && dragItem != null)
            {
                var index = dragItem.gameObject.transform.GetSiblingIndex();
                dragItem.gameObject.transform.SetSiblingIndex(selectedItem.gameObject.transform.GetSiblingIndex());
                selectedItem.gameObject.transform.SetSiblingIndex(index);
            }
            else if (selectedItem == null)
            {
                Item item = dragItem.GetAndClearItem();
                inventory.Remove(item);
                onInventoryChange();
                item.gameObject.SetActive(true);
                item.transform.position = transform.position;
            }



            var color = dragItem.icon.color;
            color.a = 1f;
            dragItem.icon.color = color;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            dragItem = null;
            Drag = false;
        }

        private void OnEnter(InventorySlot slot)
        {            
            selectedItem = slot;
        }

        private void OnExit(InventorySlot slot)
        {          
            if (selectedItem != null)
                selectedItem = null;
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
