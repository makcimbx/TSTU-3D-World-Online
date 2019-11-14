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
        private InventorySlot[] slots;

        private event Action OnInventoryChange;

        public List<Item> inventory = new List<Item>();     

        private InventorySlot dragItem = null;
        private InventorySlot selectedItem = null;

        private StateInventory state = StateInventory.none;

        private enum StateInventory
        {
            none,
            drag,
            dragAndSelect,
            select
        }
               
        internal void AddItem(Item item)
        {
            inventory.Add(item);       
            item.gameObject.SetActive(false);
            OnInventoryChange();
        }

        internal Item GetAndClearItem()
        {          
            return GetAndClearItem(selectedItem);
        }

        private Item GetAndClearItem(InventorySlot slot)
        {
            Item item = slot.GetAndClearItem();
            inventory.Remove(item);

            if (!item.instantiate)
            {
                var obj = Instantiate(item.gameObject, transform.position, Quaternion.identity);
                obj.GetComponent<Item>().instantiate = true;
            }
            else
            {
                item.gameObject.SetActive(true);
                item.transform.position = transform.position;
            }

            OnInventoryChange();

            return item;
        }

        internal void SetInventoryPanel(GameObject panel)
        {
            this.panel = panel;
        }

        internal Item GetItem()
        {
            Item item = selectedItem.GetItem();
            OnInventoryChange();

            return item;            
        }

        void Start()
        {
          
            slots = panel.GetComponentsInChildren<InventorySlot>();


            foreach (var item in slots)
            {                            
                item.onButtonDrag += OnDrag;
                item.onButtonDragEnd += OnDragEnd;
                item.onPointerEnter += OnEnter;
                item.onPointerExit += OnExit;
            }
            OnInventoryChange += UpdateUI;
            OnInventoryChange();
        }
        
        private void OnDrag(InventorySlot slot)
        {

            if (state == StateInventory.select)
            {
                if (!slot.isEmpty)
                {
                    dragItem = slot;

                    var color = dragItem.icon.color;
                    color.a = 0.3f;
                    dragItem.icon.color = color;
                    Cursor.SetCursor((Texture2D)dragItem.icon.sprite.texture, Vector2.zero, CursorMode.ForceSoftware);

                    state = StateInventory.dragAndSelect;
                }
            }
            Debug.Log("OnDrag " + state);

            //dragItem.icon.transform.position = Input.mousePosition;

        }

        private void OnDragEnd(InventorySlot slot)
        {

            //dragItem.icon.transform.position = dragItem.transform.position;
            if (state == StateInventory.dragAndSelect)
            {
                var index = dragItem.gameObject.transform.GetSiblingIndex();
                dragItem.gameObject.transform.SetSiblingIndex(selectedItem.gameObject.transform.GetSiblingIndex());
                selectedItem.gameObject.transform.SetSiblingIndex(index);
                state = StateInventory.select;
            }
            else if (state == StateInventory.drag)
            {
                Item item = GetAndClearItem(dragItem);
                state = StateInventory.none;
            }

            if(dragItem != null)
            {
                var color = dragItem.icon.color;
                color.a = 1f;
                dragItem.icon.color = color;
            }
            
                             
            dragItem = null;
            Debug.Log("OnDragEnd " + state);

            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        private void OnEnter(InventorySlot slot)
        {

            selectedItem = slot;

            if (state == StateInventory.none)
                state = StateInventory.select;
            else if (state == StateInventory.drag)
                state = StateInventory.dragAndSelect;
            Debug.Log("OnEnter " + state);

        }

        private void OnExit(InventorySlot slot)
        {

            selectedItem = null;

            if (state == StateInventory.dragAndSelect)
                state = StateInventory.drag;
            else if (state == StateInventory.select)
                state = StateInventory.none;
            Debug.Log("OnExit " + state);

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
            
        internal void SetActive(bool v)
        {
            panel?.gameObject.SetActive(v);
        }




    }
}
