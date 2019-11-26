using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace TSTU.Controller
{
    public class ChatController : MonoBehaviour
    {
        [SerializeField] private InputField input;
        [SerializeField] private Text text;
        [SerializeField] private Scrollbar scrollbar;

        [SerializeField] private bool debug = false;

        internal void Initialization()
        {
            GameController.Instance.OnSecondPassed.Subscribe(x => UpdateChat()).AddTo(this);
            text.text = "";

        }


        internal async void Submit()
        {

            await GameController.Instance.GameServer.AddMessageToChat(input.text);

            if (debug)
                Debug.Log("ChatSubmit");
            input.text = "";
        }

        internal async void UpdateChat()
        {

            await GameController.Instance.GameServer.GetMessagesFromChat(20);

            string chat = "";

            foreach (var item in GameController.Instance.GameServer.ChatMessagesList)
            {
                chat += $"{item} \n";
            }

            text.text = chat;

            if (debug)
                Debug.Log("UpdateChat" + GameController.Instance.GameServer.ChatMessagesList.Count);
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
}