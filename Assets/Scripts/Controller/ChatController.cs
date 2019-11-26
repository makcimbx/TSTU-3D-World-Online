using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [SerializeField] private InputField input;
    [SerializeField] private Text text;

    
    internal void Submit()
    {

    }


    internal void SetActive(bool v)
    {
        if (v)
        {
            input.interactable = true;
            
        }
        else
        {
            input.interactable = false;

        }
    }



}
