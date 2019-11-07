using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{


    public event Action OnForward, OnLeft, OnRight, OnBack;
    public event Axis OnAxisHorizontal, OnAxisVertical;
    public delegate void Axis(float Axis);



    public KeyCode Forward = KeyCode.W,
                Left = KeyCode.A,
                Right = KeyCode.D,
                Back = KeyCode.S;

    public string AxisX = "Horizontal",
        AxisY = "Vertical";

    private void Start()
    {
        UpdateManager.OnUpdate += () => ControlUpdate();
    }
    void ControlUpdate()
    {
        if (Input.GetKey(Forward))
            OnForward();
        if (Input.GetKey(Left))
            OnLeft();
        if (Input.GetKey(Right))
            OnRight();
        if (Input.GetKey(Back))
            OnBack();

        OnAxisHorizontal(Input.GetAxis("Mouse X"));
        OnAxisVertical(Input.GetAxis("Mouse Y"));

    }
}
