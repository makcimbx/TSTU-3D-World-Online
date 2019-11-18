using System;
using UnityEngine;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryController : MonoBehaviour
    {
        private GameObject playerPanel;
        private GameObject traderPanel;

        private Text[] texts;

        private InventorySlot[] slots;
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

        internal Item GetSelectedItem()
        {
            return GetItem(selectedItem);
        }

        private Item GetItem(InventorySlot slot)
        {
            Item item = slot.GetAndClearItem();
            Inventory.instance.Remove(item);
            var obj = Instantiate(item.prefab, transform.position, Quaternion.identity);

            return item;
        }

        internal void SetInventoryPanels(GameObject playerPanel, GameObject traderPanel)
        {
            this.playerPanel = playerPanel;
            this.traderPanel = traderPanel;
        }

        private void Start()
        {

            slots = playerPanel.GetComponentsInChildren<InventorySlot>();
            texts = playerPanel.GetComponentsInChildren<Text>();

            foreach (var item in slots)
            {
                item.onButtonDrag += OnDrag;
                item.onButtonDragEnd += OnDragEnd;
                item.onPointerEnter += OnEnter;
                item.onPointerExit += OnExit;
            }
            Inventory.instance.OnInventoryChange += UpdateUI;
            UpdateUI();
        }

        #region OnMouse

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
                GetItem(dragItem);
                state = StateInventory.none;
            }

            if (dragItem != null)
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

        #endregion
        
        public void SetTrader(Trader trader)
        {

        }

        public void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < Inventory.instance.Count)
                {
                    slots[i].AddItem(Inventory.instance.items[i]);
                }
                else
                {
                    slots[i].ClearSlot();
                }
            }
            texts[2].text = $"{Inventory.instance.Money}";
        }

        internal void SetActive(bool v)
        {
            playerPanel?.gameObject.SetActive(v);
        }

        internal void StartTrading(Trader trader)
        {
            throw new NotImplementedException();
        }
    }
}
