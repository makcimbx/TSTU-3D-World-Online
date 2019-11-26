using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField] private InputField input;
    [SerializeField] private Text text;
    [SerializeField] private Scrollbar scrollbar;

    [SerializeField] private bool debug = false;

    internal void Submit()
    {
        if(debug)
            Debug.Log("ChatSubmit");

        input.text = "";      
    }

    internal void UpdateChat()
    {
        if (debug)
            Debug.Log("UpdateChat");
    }



    internal void SetActive(bool v)
    {
        input.gameObject.SetActive(v);
        scrollbar.interactable = v;
        if (!v)
        {
            input.text = "";
        }

            SetTextAlpha(v);
    }

    private void SetTextAlpha(bool v)
    {
        float a = v ? 1f : 0.4f;
        
        var color = text.color;
        color.a = a;
        text.color = color;        
    }
}
