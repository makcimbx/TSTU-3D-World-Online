using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemButton : EventTrigger
{
    public event Action onButtonDrag, onButtonDragEnd;

    public override void OnDrag(PointerEventData eventData)
    {
        onButtonDrag();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        onButtonDragEnd();
    }
}

