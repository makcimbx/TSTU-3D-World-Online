using System;
using UnityEngine;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private bool debug = false;

        private InventoryPanel playerPanel;
        private InventoryPanel traderPanel;
        private InventoryPanel buyPanel;
        private InventoryPanel sellPanel;
                  
        private Trader trader = null;

        private Button trade;
        private Button back;

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
            Inventory.instance.Remove(item, false, transform.position);

            return item;
        }
        
        internal void SetInventoryPanels(
            InventoryPanel playerPanel,
            InventoryPanel traderPanel, 
            InventoryPanel buyPanel, 
            InventoryPanel sellPanel)
        {
            this.playerPanel = playerPanel;
            this.traderPanel = traderPanel;
            this.buyPanel = buyPanel;
            this.sellPanel = sellPanel;

            Initialization();
        }

        private void Initialization()
        {
            PanelInit(playerPanel, InventoryPanel.Panel.Player);
            PanelInit(traderPanel, InventoryPanel.Panel.Trader);
            PanelInit(buyPanel, InventoryPanel.Panel.Buy);
            PanelInit(sellPanel, InventoryPanel.Panel.Sell);
                      
            UpdateUI();
            CloseAll();
        }

        internal void SetButtons(Button back, Button trade)
        {
            this.trade = trade;
            this.back = back;

            back.gameObject.SetActive(false);
            trade.gameObject.SetActive(false);
        }

        private void PanelInit(InventoryPanel panel, InventoryPanel.Panel namePanel)
        {
            panel.State = namePanel;

            panel.Slots = panel.itemParent.GetComponentsInChildren<InventorySlot>();

            foreach (var item in panel.Slots)
            {
                item.onButtonDrag += OnDrag;
                item.onButtonDragEnd += OnDragEnd;
                item.onPointerEnter += OnEnter;
                item.onPointerExit += OnExit;
                item.panel = namePanel;
            }
            if (debug)
                Debug.Log("PanelInit " + panel.name);
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

                    if (dragItem.panel == InventoryPanel.Panel.Player || 
                        dragItem.panel == InventoryPanel.Panel.Sell)
                    {
                        buyPanel.Curtain = true;
                        traderPanel.Curtain = true;
                    }
                    else
                    {
                        playerPanel.Curtain = true;
                        sellPanel.Curtain = true;
                    }
                }
            }

            if(debug)
                Debug.Log("OnDrag " + dragState);
        }

        private void OnDragEnd(InventorySlot slot)
        {
            if (dragItem.panel == InventoryPanel.Panel.Player ||
                dragItem.panel == InventoryPanel.Panel.Sell)
            {
                buyPanel.Curtain = false;
                traderPanel.Curtain = false;
            }
            else
            {
                playerPanel.Curtain = false;
                sellPanel.Curtain = false;
            }
            if (dragState == DragState.dragAndSelect)
            {
              

                if (selectedItem.panel == InventoryPanel.Panel.Player)
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
                else if (selectedItem.panel == InventoryPanel.Panel.Sell)
                {
                    Item drag = dragItem.GetAndClearItem();
                    Item select = selectedItem.GetAndClearItem();
                    if (select != null)
                        dragItem.AddItem(select);
                    selectedItem.AddItem(drag);

                }
                else if (selectedItem.panel == InventoryPanel.Panel.Buy ||
                    selectedItem.panel == InventoryPanel.Panel.Trader)
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
            var inventory = Inventory.instance.items;

            for (int i = 0; i < playerPanel.Slots.Length; i++)
            {
                if (inventory.ContainsKey(i))
                {
                    playerPanel.Slots[i].AddItem(Inventory.instance.items[i]);
                }
                else
                {
                    playerPanel.Slots[i].ClearSlot();
                }
            }

            playerPanel.Money = Inventory.instance.Money;
            var traderInventory = trader.Items;
            if (inventoryState == InventoryState.Trade)
            {
                for (int i = 0; i < traderPanel.Slots.Length; i++)
                {
                    if (i < traderInventory.Length)
                    {
                        traderPanel.Slots[i].AddItem(traderInventory[i]);

                    }
                    else
                    {
                        traderPanel.Slots[i].ClearSlot();
                    }
                }

            }

            playerPanel.Money = Inventory.instance.Money;
            if (debug)
                Debug.Log("UpdateUI");
        }

        internal void Trade()
        {
            foreach (var slot in sellPanel.Slots)
            {
                if (!slot.isEmpty)
                {
                    var item = slot.GetAndClearItem();

                }
                    
            }
            Debug.Log("Обмен с торговцем" + trader.gameObject.name);
        }

        internal void OpenInventory()
        {
            if (inventoryState == InventoryState.None)
            {
                inventoryState = InventoryState.Inventory;
                if(playerPanel != null)
                    playerPanel.Active = true;
                UpdateUI();
            }
        }

        internal void CloseAll()
        {
            if (inventoryState == InventoryState.Inventory)
            {
                inventoryState = InventoryState.None;
                if (playerPanel != null)
                    playerPanel.Active = false;
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
            else
                SetActiveTrading(false);
        }

        internal void StartTrading(Trader trader)
        {
            if (inventoryState == InventoryState.None)
            {
                inventoryState = InventoryState.Trade;
                this.trader = trader;
                Inventory.instance.OnInventoryChange += trader.UpdateInventory;

                StateActivePanel();

                UpdateUI();

               
            }

            if (debug)
                Debug.Log("Старт торговли");
        }

        private void EndTrading()
        {

            if (dragState == DragState.drag || 
                dragState == DragState.dragAndSelect )
            {
                OnDragEnd(null);
            }

            if (inventoryState == InventoryState.Trade)
            {

                for (int i = 0; i < sellPanel.Slots.Length; i++)
                {
                    if (!sellPanel.Slots[i].isEmpty)
                    {
                        InventorySlot slot = null;
                        
                        

                        //for (int j = 0; j < playerPanel.Slots.Length; j++)
                        //    if (playerPanel.Slots[j].isEmpty)
                        //        slot = playerPanel.Slots[j];

                        //if (slot != null)
                        //    slot.AddItem(sellPanel.Slots[i].GetAndClearItem());
                        //else
                        //    GetItem(sellPanel.Slots[i]);
                    }
                }
              

                inventoryState = InventoryState.None;
                UpdateUI();
                Inventory.instance.OnInventoryChange -= trader.UpdateInventory;
                trader = null;
                StateActivePanel();
                
            }

            if (debug)
                Debug.Log("Конец торговли");
        }

        private void SetActiveTrading(bool v)
        {
            playerPanel.Active =
                traderPanel.Active =
                buyPanel.Active =
                sellPanel.Active = v;
        }

        private void StateActivePanel()
        {
            if (inventoryState == InventoryState.None)
            {
                SetActiveTrading(false);
                back.gameObject.SetActive(false);
                trade.gameObject.SetActive(false);
            }
            else if (inventoryState == InventoryState.Trade)
            {
                SetActiveTrading(true);
                back.gameObject.SetActive(true);
                trade.gameObject.SetActive(true);
            }
            else if (inventoryState == InventoryState.Inventory)
            {
                playerPanel.Active = true;

            }
            else if (inventoryState == InventoryState.Search)
            {
                if (debug)
                    Debug.Log("Поиск предметов ещё не готов");
            }
        }
    }
}
