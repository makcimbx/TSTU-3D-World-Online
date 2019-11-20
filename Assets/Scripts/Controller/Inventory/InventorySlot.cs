using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TSTU.Controller
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField]
        public Image icon;
        [SerializeField]
        public GameObject itemButton;
        private Item item;
        
        public bool isEmpty
        {
            get
            {
                return item == null;
            }
        }

        public InventoryPanel.Panel panel { get; set; } = InventoryPanel.Panel.None;

        public event Action<InventorySlot> onButtonDrag, onButtonDragEnd, onPointerEnter, onPointerExit;

        private void Start()
        {
            EventTrigger trigger = itemButton.GetComponent<EventTrigger>();

            EventTrigger.Entry drag = new EventTrigger.Entry(),
                               dragEnd = new EventTrigger.Entry(),
                               pointerEnter = new EventTrigger.Entry(),
                               pointerExit = new EventTrigger.Entry();


            drag.eventID = EventTriggerType.Drag;
            dragEnd.eventID = EventTriggerType.EndDrag;
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerExit.eventID = EventTriggerType.PointerExit;

            drag.callback.AddListener((x) => OnButtonDrag());
            dragEnd.callback.AddListener((x) => OnButtonDragEnd());
            pointerEnter.callback.AddListener((x) => OnPointerEnter());
            pointerExit.callback.AddListener((x) => OnPointerExit());

            trigger.triggers.AddRange(
                new EventTrigger.Entry[] { drag, dragEnd, pointerEnter, pointerExit });
        }

        public void OnButtonDrag()
        {
            //if (icon != null && item != null)        
            onButtonDrag?.Invoke(this);
        }

        public void OnButtonDragEnd()
        {
            //if (icon != null && item != null)               
            onButtonDragEnd?.Invoke(this);

        }

        public void OnPointerEnter()
        {
            //if (icon != null && item != null)
            onPointerEnter?.Invoke(this);
        }

        public void OnPointerExit()
        {
            //if (icon != null && item != null)
            onPointerExit?.Invoke(this);
        }

        public void AddItem(Item item)
        {
            this.item = item;
            icon.sprite = item.icon;
            icon.enabled = true;
        }

        public Item GetItem()
        {
            Item item = this.item;
            return item;
        }


        public Item GetAndClearItem()
        {
            Item item = this.item;
            ClearSlot();
            return item;
        }

        public void ClearSlot()
        {
            item = null;
            icon.sprite = null;
            icon.enabled = false;
        }


    }
}