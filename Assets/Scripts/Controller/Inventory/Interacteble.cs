using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
namespace TSTU.Controller
{
    public class Interacteble : MonoBehaviour
    {       
        public bool isInteracteble = true;

        public virtual void Interact()
        {
            Debug.Log("Взаимодействие с " + name);
        }
  
    }
}