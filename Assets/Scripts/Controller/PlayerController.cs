using UnityEngine;
using System;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(
        typeof(InventoryController), 
        typeof(FirstPersonController))]

    public class PlayerController : MonoBehaviour
    {
        [Space(10)]
        [Header("Панель инвентаря")]
        [SerializeField] private GameObject playerPanel;

        [Space(10)]
        [Header("Панель товаров")]
        [SerializeField] private GameObject traderPanel;

        [Space(10)]
        [Header("Панель покупки")]
        [SerializeField] private GameObject buyPanel;

        [Space(10)]
        [Header("Панель продажи")]
        [SerializeField] private GameObject sellPanel;

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

        private StateView stateView = StateView.None;
               
        public enum StateView
        {
            None,
            Inventory,
            Menu,
            Trade,
            Search
        }


        private void Start()
        {
            inventoryController = GetComponent<InventoryController>();
            firstPersonController = GetComponent<FirstPersonController>();

            Cursor.lockState = CursorLockMode.Locked;
            inventoryController.SetInventoryPanels(playerPanel, traderPanel, buyPanel, sellPanel);
            inventoryController.CloseAll();

            back.gameObject.SetActive(false);
            trade.gameObject.SetActive(false);

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
                    inventoryController.OpenInventory();
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
                else
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
        }
        #endregion



        public void StartTrading(Trader trader)
        {
            stateView = StateView.Trade;
            Cursor.lockState = CursorLockMode.None;
            firstPersonController.enabled = false;
            inventoryController.StartTrading(trader);
            back.gameObject.SetActive(true);
            trade.gameObject.SetActive(true);

        }

        public void EndTrading()
        {
            stateView = StateView.None;
            Cursor.lockState = CursorLockMode.Locked;
            firstPersonController.enabled = true;
            inventoryController.CloseAll();
            back.gameObject.SetActive(false);
            trade.gameObject.SetActive(false);
        }
    }

}