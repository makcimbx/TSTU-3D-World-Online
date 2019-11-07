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


        public void OnSignIn()
        {
            Debug.Log($"Ввод параши { Login.text} - {Password.text}");
            SceneManager.LoadScene(1,LoadSceneMode.Single);
        }
    }

}