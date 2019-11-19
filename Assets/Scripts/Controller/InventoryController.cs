using System;
using UnityEngine;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private bool debug = false;

        private GameObject playerPanel = null;
        private GameObject traderPanel = null;
        private GameObject buyPanel = null;
        private GameObject sellPanel = null;

        private Trader trader = null;

        private Text[] playerText;
        private Text[] traderText = null;
        private Text[] buyText = null;
        private Text[] sellText = null;

        private InventorySlot[] playerSlots = null;
        private InventorySlot[] buySlots = null;
        private InventorySlot[] sellSlots = null;
        private InventorySlot[] traderSlots = null;

        private InventorySlot dragItem = null;
        private InventorySlot selectedItem = null;

        [SerializeField] private DragState dragState = DragState.none;
        [SerializeField] private InventoryState inventoryState = InventoryState.None;

        private enum DragState
        {
            none,
            drag,
            dragAndSelect,
            select
        }

        private enum InventoryState
        {
            None,
            Inventory,            
            Trade,
            Search
        }


        private void Start()
        {
            Inventory.instance.OnInventoryChange += UpdateUI;
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

            Initialization();
        }

        internal void SetInventoryPanels(GameObject playerPanel, GameObject traderPanel, GameObject buyPanel, GameObject sellPanel)
        {
            this.playerPanel = playerPanel;
            this.traderPanel = traderPanel;
            this.buyPanel = buyPanel;
            this.sellPanel = sellPanel;

            Initialization();
        }

        private void Initialization()
        {
            PanelInit(playerPanel, ref playerSlots, ref playerText , InventorySlot.Panel.Player);
            PanelInit(traderPanel, ref traderSlots, ref traderText, InventorySlot.Panel.Trader);
            PanelInit(buyPanel, ref buySlots, ref buyText, InventorySlot.Panel.Buy);
            PanelInit(sellPanel, ref sellSlots, ref sellText, InventorySlot.Panel.Sell);
            StateActivePanel();
        }


        private void PanelInit(GameObject panel, ref InventorySlot[] inventorySlots, ref Text[] texts, InventorySlot.Panel namePanel)
        {
            inventorySlots = panel?.GetComponentsInChildren<InventorySlot>();
            
            texts = panel?.GetComponentsInChildren<Text>();

            if (inventorySlots != null)
            {
                foreach (var item in inventorySlots)
                {
                    item.onButtonDrag += OnDrag;
                    item.onButtonDragEnd += OnDragEnd;
                    item.onPointerEnter += OnEnter;
                    item.onPointerExit += OnExit;
                    item.panel = namePanel;
                }
            }
            UpdateUI();
        }

        #region OnMouse

        private void OnDrag(InventorySlot slot)
        {

            if (dragState == DragState.select)
            {
                if (!slot.isEmpty)
                {
                    dragItem = slot;

                    var color = dragItem.icon.color;
                    color.a = 0.3f;
                    dragItem.icon.color = color;
                    Cursor.SetCursor((Texture2D)dragItem.icon.sprite.texture, Vector2.zero, CursorMode.ForceSoftware);

                    dragState = DragState.dragAndSelect;
                }
            }

            if(debug)
                Debug.Log("OnDrag " + dragState);
        }

        private void OnDragEnd(InventorySlot slot)
        {
            if (dragState == DragState.dragAndSelect)
            {                
                if (selectedItem.panel == InventorySlot.Panel.Player)
                {
                    //var index = dragItem.gameObject.transform.GetSiblingIndex();
                    //dragItem.gameObject.transform.SetSiblingIndex(selectedItem.gameObject.transform.GetSiblingIndex());
                    //selectedItem.gameObject.transform.SetSiblingIndex(index);
                    Item drag = dragItem.GetAndClearItem();
                    Item select = selectedItem.GetAndClearItem();
                    if (select != null)
                        dragItem.AddItem(select);
                    selectedItem.AddItem(drag);
                }
                else if (selectedItem.panel == InventorySlot.Panel.Sell)
                {
                    Item drag = dragItem.GetAndClearItem();
                    Item select = selectedItem.GetAndClearItem();
                    if (select != null)
                        dragItem.AddItem(select);
                    selectedItem.AddItem(drag);

                }

                dragState = DragState.select;
            }
            else if (dragState == DragState.drag)
            {
                if (inventoryState == InventoryState.Inventory) 
                    GetItem(dragItem);
                dragState = DragState.none;
            }
            

            if (dragItem != null)
            {
                var color = dragItem.icon.color;
                color.a = 1f;
                dragItem.icon.color = color;
            }

            dragItem = null;        
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);

            if (debug)
                Debug.Log("OnDragEnd " + dragState);
        }

        private void OnEnter(InventorySlot slot)
        {

            selectedItem = slot;

            if (dragState == DragState.none)
                dragState = DragState.select;
            else if (dragState == DragState.drag)
                dragState = DragState.dragAndSelect;
            if (debug)
                Debug.Log("OnEnter " + dragState);

        }

        private void OnExit(InventorySlot slot)
        {

            selectedItem = null;

            if (dragState == DragState.dragAndSelect)
                dragState = DragState.drag;
            else if (dragState == DragState.select)
                dragState = DragState.none;

            if (debug)
                Debug.Log("OnExit " + dragState);

        }

        #endregion


        public void UpdateUI()
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (i < Inventory.instance.Count)
                {
                    playerSlots[i].AddItem(Inventory.instance.items[i]);
                }
                else
                {
                    playerSlots[i].ClearSlot();
                }
            }
            playerText[2].text = $"{Inventory.instance.Money}";

            if (debug)
                Debug.Log("UpdateUI ");
        }



        internal void OpenInventory()
        {
            if (inventoryState == InventoryState.None)
            {
                inventoryState = InventoryState.Inventory;
                playerPanel?.SetActive(true);
            }
        }

        internal void CloseAll()
        {
            if (inventoryState == InventoryState.Inventory)
            {
                inventoryState = InventoryState.None;
                playerPanel?.SetActive(false);
            }
            else if (inventoryState == InventoryState.Trade)
            {
                EndTrading();
            }
            else if(inventoryState == InventoryState.Search)
            {
                if (debug)
                    Debug.Log("Поиск предметов ещё не готов");
            }
        }

        internal void StartTrading(Trader trader)
        {
            if (inventoryState == InventoryState.None)
            {
                inventoryState = InventoryState.Trade;
                this.trader = trader;
                StateActivePanel();
                UpdateUI();
            }

            if (debug)
                Debug.Log("Старт торговли");
        }

        private void EndTrading()
        {
            if (inventoryState == InventoryState.Trade)
            {

                for (int i = 0; i < sellSlots.Length; i++)
                {
                    if (!sellSlots[i].isEmpty)
                    {
                        InventorySlot slot = null;

                        for (int j = 0; j < playerSlots.Length; j++)
                            if (playerSlots[j].isEmpty)
                                slot = playerSlots[j];

                        if (slot != null)
                            slot.AddItem(sellSlots[i].GetAndClearItem());
                        else
                            GetItem(sellSlots[i]);
                    }
                }
              

                inventoryState = InventoryState.None;
                trader = null;
                StateActivePanel();
                UpdateUI();
            }

            if (debug)
                Debug.Log("Старт конец торговли");
        }

        private void SetActiveTrading(bool v)
        {
            playerPanel.SetActive(v);
            traderPanel.SetActive(v);
            buyPanel.SetActive(v);
            sellPanel.SetActive(v);
        }
        
        private void StateActivePanel()
        {
            if (inventoryState == InventoryState.None)
            {
                SetActiveTrading(false);
            }
            else if (inventoryState == InventoryState.Trade)
            {
                SetActiveTrading(true);
            }
            else if (inventoryState == InventoryState.Inventory)
            {
                playerPanel.SetActive(true);
            }
            else if (inventoryState == InventoryState.Search)
            {
                if (debug)
                    Debug.Log("Поиск предметов ещё не готов");
            }
        }
    }
}
