using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemButton : EventTrigger
{
    public event Action onButtonDrag, onButtonDragEnd, onPointerEnter, onPointerExit;

    public InventorySlot slot;



    public override void OnDrag(PointerEventData eventData)
    {
        onButtonDrag?.Invoke();

        
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        onButtonDragEnd?.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {       
        onPointerEnter?.Invoke();
    }
    public override void OnPointerExit(PointerEventData eventData)
    {

        onPointerExit?.Invoke();
    }

}

