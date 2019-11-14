using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemButton : EventTrigger
{
    public event Action onButtonDrag, onButtonDragEnd, onPointerEnter, onPointerExit;

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

    public override void OnSelect(BaseEventData eventData)
    {
        Debug.Log("OnSelect");
    }

    public override void OnMove(AxisEventData eventData)
    {
        Debug.Log("OnMove");
    }

    public override void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("OnSubmit");
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Debug.Log("OnDeselect");
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        Debug.Log("OnUpdateSelected");
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Debug.Log("OnInitializePotentialDrag");
    }

}

