using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSTU.Controller
{
    public class TickController : MonoBehaviour
    {
        private void Update()
        {
            GameController.Instance.Tick(Time.deltaTime);
        }
    }
}
