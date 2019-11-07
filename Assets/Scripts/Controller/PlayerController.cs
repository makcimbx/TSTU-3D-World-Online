using UnityEngine;
using System;
using System.Collections.Generic;

namespace TSTU.Controller
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private GameObject cube;

        private void Start()
        {
            LeanTween.rotate(cube,new Vector3(0,10000000,0),500);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(LeanTween.isTweening(cube))
                    LeanTween.cancel(cube);
                else
                    LeanTween.rotate(cube, new Vector3(0, 10000000, 0), 500);

            }
        }

    }

}