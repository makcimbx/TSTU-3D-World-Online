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




        [Header("Панель инвентаря")]
        [SerializeField] private GameObject playerPanel;
        [Header("Панель отображения торговцев")]
        [SerializeField] private GameObject traderPanel;
        [Header("Точка переноса объектов")]
        [SerializeField] private Transform pointMove;
        private Transform moveObject;
        private bool IsСarry = false;


        private FirstPersonController firstPersonController;
        private InventoryController inventoryController;

        private StateView stateView = StateView.None;

       

        public enum StateView
        {
            None,
            Inventory,
            Menu
        }


        private void Start()
        {
            inventoryController = GetComponent<InventoryController>();
            firstPersonController = GetComponent<FirstPersonController>();

            Cursor.lockState = CursorLockMode.Locked;
            inventoryController.SetInventoryPanels(playerPanel, traderPanel);
            inventoryController.SetActive(false);
            

            
        }

        #region Update
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (stateView == StateView.Menu)
                {                    
                    Cursor.lockState = CursorLockMode.Locked;
                    firstPersonController.enabled = (true);
                    stateView = StateView.None;
                }
                else if (stateView == StateView.Inventory)
                {
                    inventoryController.SetActive(false);
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.None;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    stateView = StateView.Menu;
                    firstPersonController.enabled = (false);
                    Cursor.lockState = CursorLockMode.None;
                }

            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {

                if (stateView == StateView.Inventory)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    inventoryController.SetActive(false);
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.None;
                }
                else if (stateView == StateView.None)
                {
                    stateView = StateView.Inventory;
                    firstPersonController.SetRotationStatus(false);
                    Cursor.lockState = CursorLockMode.None;
                    inventoryController.SetActive(true);
                }

            }

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
                                    item.Interact();
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if(stateView == StateView.Inventory)
                    inventoryController.GetSelectedItem();              
            }



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
                                    moveObject = item.transform;
                                    IsСarry = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        IsСarry = false;
                        moveObject = null;

                    }
                    
                }                  
            }


            if (IsСarry)
            {
                moveObject.position = pointMove.position;
            }
           
        }
        #endregion



        public void StartTrading(Trader trader)
        {

            inventoryController.StartTrading(trader);
        }

        public void EndTrading()
        {

        }
    }

}