using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TSTU.Controller
{
    public class LoadScreenController : MonoBehaviour
    {
        [SerializeField] private Button SignIn;
        [SerializeField] private Button Registration;
        [SerializeField] private Button Back;



        [Space(10)]
        [SerializeField] private InputField Login;
        [SerializeField] private InputField Password;
        [SerializeField] private InputField Password2;


        [Space(10)]
        [SerializeField] private GameObject InfoPanel;
        [SerializeField] private Text InfoPanelText;




        private StateView stateView = StateView.signIn;

        private enum StateView
        {
            signIn,
            registration
        }

        private void Start()
        {
            SignIn.onClick.AddListener(OnSignIn);
            Registration.onClick.AddListener(OnRegistration);
            Back.onClick.AddListener(OnBack);


        }


        public async void OnSignIn()
        {
            Debug.Log($"Отправка сообщения { Login.text} - {Password.text}");
            var answer = await GameController.Instance.GameServer.Login(Login.text, Password.text);
            if (answer)
            {
                Debug.Log($"Вход совершен");
                SceneManager.LoadScene(1, LoadSceneMode.Single);                
            }
            else
            {
                Debug.Log($"В базе данных нет такого пользователя");
            }
        }
                

        public async void OnRegistration()
        {
            if(stateView == StateView.signIn)
            {
                SignIn.gameObject.SetActive(false);
                Back.gameObject.SetActive(true);
                Password2.gameObject.SetActive(true);
                InfoPanelText.text = "Регистрация";
                stateView = StateView.registration;

            }
            else if (stateView == StateView.registration)
            {
                if(Password.text == Password2.text)
                {
                    var answer = await GameController.Instance.GameServer.Registration(Login.text, Password.text);
                    if (answer)
                    {
                        SignIn.gameObject.SetActive(true);
                        Back.gameObject.SetActive(false);
                        Password2.gameObject.SetActive(false);
                        InfoPanelText.text = "Вход";
                        stateView = StateView.signIn;
                        Debug.Log("Регистрация прошла успешно.");

                    }
                    else
                    {
                        Debug.Log("Пользователь с таким логином уже зарегестрирован");
                    }
                }
                else
                {
                    Debug.Log("Введёные пароли не совпадают");
                }



            }
        }


        public void OnBack()
        {
            SignIn.gameObject.SetActive(true);
            Back.gameObject.SetActive(false);
            Password2.gameObject.SetActive(false);
            InfoPanelText.text = "Вход";
            stateView = StateView.signIn;
        }
    }

}