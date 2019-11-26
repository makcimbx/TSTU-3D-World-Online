using UnityEngine;
using System;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(
        typeof(InventoryController), 
        typeof(FirstPersonController),
        typeof(ChatController)
        )]

    public class PlayerController : MonoBehaviour
    {
        [Space(10)]
        [Header("Панель инвентаря")]
        [SerializeField] private InventoryPanel playerPanel;

        [Space(10)]
        [Header("Панель товаров")]
        [SerializeField] private InventoryPanel traderPanel;

        [Space(10)]
        [Header("Панель покупки")]
        [SerializeField] private InventoryPanel buyPanel;

        [Space(10)]
        [Header("Панель продажи")]
        [SerializeField] private InventoryPanel sellPanel;

        [Space(10)]
        [Header("Точка для переноса объектов")]
        [SerializeField] private Transform pointMove;

        [Space(10)]
        [Header("Кнопка назад")]
        [SerializeField] private Button back;

        [Space(10)]
        [Header("Кнопка торговать")]
        [SerializeField] private Button trade;


        private Transform moveObject;
        private Rigidbody moveObjectRb;
        private bool IsСarry = false;


        private FirstPersonController firstPersonController;
        private InventoryController inventoryController;
        private ChatController chatController;

        [SerializeField] private StateView stateView = StateView.None;
               
        public enum StateView
        {
            None,
            Inventory,
            Menu,
            Trade,
            Search,
            Chat
        }


        private void Start()
        {
            inventoryController = GetComponent<InventoryController>();
            firstPersonController = GetComponent<FirstPersonController>();
            chatController = GetComponent<ChatController>();
            chatController.SetActive(false);

            
            inventoryController.SetInventoryPanels(playerPanel, traderPanel, buyPanel, sellPanel);
            inventoryController.SetButtons(back, trade);




            Cursor.lockState = CursorLockMode.Locked;
        }

        #region Update
        private void Update()
        {
            #region Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (stateView == StateView.Menu)
                {                    
                    Cursor.lockState = CursorLockMode.Locked;
                    firstPersonController.enabled = true;
                    stateView = StateView.None;
                }
                else if (stateView == StateView.Inventory)
                {
                    inventoryController.CloseAll();
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.None;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else if (stateView == StateView.Trade)
                {
                    EndTrading();
                    firstPersonController.SetRotationStatus(true);
                }
                else if (stateView == StateView.Search)
                {
                   
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.None;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else if (stateView == StateView.Chat)
                {
                    chatController.SetActive(false);
                    firstPersonController.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    stateView = StateView.None;
                }
                else if (stateView == StateView.None)
                {
                    stateView = StateView.Menu;
                    firstPersonController.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                }

            }
            #endregion
            #region Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {

                if (stateView == StateView.Inventory)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    inventoryController.CloseAll();
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.None;
                }
                else if (stateView == StateView.None)
                {
                    stateView = StateView.Inventory;
                    firstPersonController.SetRotationStatus(false);
                    Cursor.lockState = CursorLockMode.None;
                    inventoryController.OpenInventory();
                }
                else if (stateView == StateView.Trade)
                {
                    EndTrading();
                    stateView = StateView.None;
                }

            }
            #endregion
            #region F
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (stateView == StateView.None)
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.distance <= 3.5f)
                        {
                            Interacteble item;
                            if (hit.collider.TryGetComponent(out item))                                
                                    item.Interact(this);
                        }
                    }
                }
            }
            #endregion
            #region Delete
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if(stateView == StateView.Inventory)
                    inventoryController.GetSelectedItem();              
            }
            #endregion
            #region E
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (stateView == StateView.None)
                {
                    if (!IsСarry)
                    {
                        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit))
                        {
                            if (hit.distance <= 3.5f)
                            {
                                Interacteble item;
                                if (hit.collider.TryGetComponent(out item))
                                {
                                    if (item.isСarryble)
                                    {
                                        moveObject = item.transform;
                                        IsСarry = true;
                                     
                                        moveObject.gameObject.TryGetComponent<Rigidbody>(out moveObjectRb);
                                        moveObjectRb.Sleep();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        IsСarry = false;
                        moveObject = null;
                        moveObjectRb.WakeUp();
                        moveObjectRb = null;
                    }
                    
                }                  
            }
            if (IsСarry)
            {
                moveObject.position = pointMove.position;
            }
            #endregion
            #region OnMouseDown
            if (Input.GetMouseButtonDown(0) && stateView == StateView.None && IsСarry)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);                
                IsСarry = false;
                moveObject = null;
                moveObjectRb.WakeUp();
                moveObjectRb.AddForce(ray.direction * 50,ForceMode.Impulse);
                moveObjectRb = null;
            }
            #endregion
            #region Enter
            if (Input.GetButtonDown("Submit"))
            {
                if (stateView == StateView.Chat)
                {
                    chatController.Submit();
                }
            }
            #endregion
            #region T
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (stateView == StateView.None)
                {
                    chatController.SetActive(true);
                    firstPersonController.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    stateView = StateView.Chat;

                }
            }
            #endregion

        }
        #endregion

        public void Trading()
        {
            inventoryController.Trade();
        }


        public void StartTrading(Trader trader)
        {
            stateView = StateView.Trade;
            Cursor.lockState = CursorLockMode.None;
            firstPersonController.enabled = false;
            inventoryController.StartTrading(trader);
          

        }

        public void EndTrading()
        {
            stateView = StateView.None;
            Cursor.lockState = CursorLockMode.Locked;
            firstPersonController.enabled = true;
            inventoryController.CloseAll();
        }
    }

}