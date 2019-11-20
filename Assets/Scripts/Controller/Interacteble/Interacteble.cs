using UnityEngine;

namespace TSTU.Controller
{
    public class Interacteble : MonoBehaviour
    {       
        public bool isInteracteble = true;
        public bool isСarryble = false;

        public virtual void Interact(PlayerController player)
        {
            //Debug.Log("Взаимодействие с " + name);
        }

    }
}