using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public ItemButton itemButton;

    private Item item;    

    public event Action<InventorySlot> onButtonDrag, onButtonDragEnd, onPointerEnter, onPointerExit;
    
    private void Start()
    {
        itemButton.slot = this;

        itemButton.onButtonDrag += OnButtonDrag;
        itemButton.onButtonDragEnd += OnButtonDragEnd;
        itemButton.onPointerEnter += OnPointerEnter;
        itemButton.onPointerExit += OnPointerExit;

    }

    public void OnButtonDrag()
    {      
        if (icon != null && item != null)        
            onButtonDrag?.Invoke(this);
        icon.useGUILayout = false;
        icon.useSpriteMesh = false;



    }

    public void OnButtonDragEnd()
    {      
        if (icon != null && item != null)               
            onButtonDragEnd?.Invoke(this);

        itemButton.gameObject.layer = 0;

    }

    public void OnPointerEnter()
    {
        if (icon != null && item != null)
            onPointerEnter?.Invoke(this);
    }

    public void OnPointerExit()
    {
        if (icon != null && item != null)
            onPointerExit?.Invoke(this);
    }

    public void AddItem(Item item)
    {
        this.item = item;
        icon.sprite = item.icon;
        icon.enabled = true;
       // button.interactable = true;
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
        //button.interactable = false;
    }

   
}
