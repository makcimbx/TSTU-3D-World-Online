using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;

    public ItemButton itemButton;
    private Item item;
    
    public event Action<InventorySlot> onButtonDrag, onButtonDragEnd;

    public bool Drag = false;
    public Vector3 position { get; set; }

    private void Start()
    {
        itemButton.onButtonDrag += OnButtonDrag;
        itemButton.onButtonDrag += OnButtonDragEnd;       
    }

    public void OnButtonDrag()
    {
        if (!Drag)
        {
            Drag = true;
            position = transform.position;
        }

        if(icon != null)
            icon.transform.position = Input.mousePosition;

        onButtonDrag(this);
    }
    public void OnButtonDragEnd()
    {
        Drag = false;

        if (icon != null)
            icon.transform.position = position;
    }

    public void AddItem(Item item)
    {
        this.item = item;
        icon.sprite = item.icon;
        icon.enabled = true;
        button.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        button.interactable = false;
    }

   
}
