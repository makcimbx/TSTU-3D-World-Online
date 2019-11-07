using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    public static event Action OnUpdate;
      
    private void Update()
    {
        OnUpdate();
    }

}
