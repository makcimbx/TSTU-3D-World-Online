using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSTU.Controller
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("Занавес")]
        [SerializeField] private GameObject curtain;

        [Header("Родитель слотов")]
        [SerializeField] public GameObject itemParent;

        [Header("Текс заголовка")]
        [SerializeField] private Text handler;

        [Header("Текс")]
        [SerializeField] private Text moneyText;

        [Header("Цифра")]
        [SerializeField] private Text money;

        public InventorySlot[] Slots;

        public bool Curtain {
            get
            {
                return curtain.activeSelf;
            }
            set
            {
                curtain.SetActive(value);
            }
        }
        public long Money
        {
            get
            {
                return long.Parse(money.text);
            }
            set
            {
                money.text = $"{value}";
            }
        }


        public bool Active
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                gameObject.SetActive(value);
            }
        }


        public Panel State { get; set; } = Panel.None;
        public enum Panel
        {
            None,
            Player,
            Buy,
            Sell,
            Trader,
            Сhest
        }       

        private void Start()
        {
            Curtain = false;
        }


    }
}
