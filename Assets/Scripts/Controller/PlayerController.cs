using UnityEngine;
using System;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

namespace TSTU.Controller
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private GameObject panel;

        private StateView stateView = StateView.none;

        public enum StateView
        {
            none,
            inventory,
            esc

        }


        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            panel.SetActive(false);

        }
        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (stateView == StateView.esc)
                {
                    firstPersonController.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    stateView = StateView.none;
                }
                else if (stateView == StateView.inventory)
                {
                    panel.SetActive(false);
                    stateView = StateView.none;
                }
                else
                {
                    stateView = StateView.esc;
                    firstPersonController.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                {

                    if (stateView == StateView.inventory)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        panel.SetActive(false);
                        firstPersonController.enabled = true;
                        stateView = StateView.none;
                    }
                    else
                    {
                        stateView = StateView.inventory;
                        firstPersonController.enabled = false;
                        Cursor.lockState = CursorLockMode.None;
                        panel.SetActive(true);
                    }

                }

            }
        }
    }
}