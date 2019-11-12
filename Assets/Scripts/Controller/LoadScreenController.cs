using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TSTU.Controller
{
    public class LoadScreenController : MonoBehaviour
    {
        [SerializeField]
        private Button SignIn;
        [SerializeField]
        private InputField Login;
        [SerializeField]
        private InputField Password;


        private void Start()
        {
            SignIn.onClick.AddListener(OnSignIn);
        }


        public async void OnSignIn()
        {
            Debug.Log($"Ввод параши { Login.text} - {Password.text}");
            bool answer = false;
            answer = await GameController.Instance.GameServer.Login(Login.text, Password.text);
            if (answer)
            {
                Debug.Log($"Ввод параши асептед");
                SceneManager.LoadScene(1, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log($"Твоя параша не существует, соси");
            }
        }
    }

}