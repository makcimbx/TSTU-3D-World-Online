using UnityEngine;
using System;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

namespace TSTU.Controller
{
    [RequireComponent(typeof(InventoryController), typeof(FirstPersonController))]

    public class PlayerController : MonoBehaviour
    {
        [Header("Панель инвентаря")]
        [SerializeField] private GameObject panel;

        private FirstPersonController firstPersonController;
        private InventoryController inventoryController;



        private StateView stateView = StateView.none;

        public enum StateView
        {
            none,
            inventory,
            esc                
        }


        private void Start()
        {
            inventoryController = GetComponent<InventoryController>();
            firstPersonController = GetComponent<FirstPersonController>();

            Cursor.lockState = CursorLockMode.Locked;
            inventoryController.InventoryPanel(panel);
            inventoryController.SetActive(false);
            

            
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (stateView == StateView.esc)
                {                    
                    Cursor.lockState = CursorLockMode.Locked;
                    firstPersonController.enabled = (true);
                    stateView = StateView.none;
                }
                else if (stateView == StateView.inventory)
                {
                    inventoryController.SetActive(false);
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.none;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    stateView = StateView.esc;
                    firstPersonController.enabled = (false);
                    Cursor.lockState = CursorLockMode.None;
                }

            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {

                if (stateView == StateView.inventory)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    inventoryController.SetActive(false);
                    firstPersonController.SetRotationStatus(true);
                    stateView = StateView.none;
                }
                else if (stateView == StateView.none)
                {
                    stateView = StateView.inventory;
                    firstPersonController.SetRotationStatus(false);
                    Cursor.lockState = CursorLockMode.None;
                    inventoryController.SetActive(true);
                }

            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray,out hit))
                {
                    if (hit.distance <= 3.5f) {
                        Item item;
                        if (hit.collider.TryGetComponent<Item>(out item))
                        {
                            //Debug.Log(hit.collider.gameObject.name);

                            inventoryController.addItem(item);
                            hit.collider.gameObject.SetActive(false);
                        }
                    }
                }
            }

        }





    }

}