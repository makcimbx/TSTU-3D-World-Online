using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Control control;

    public float Speed = 1.0f;
    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;
    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;
    private float _rotationX = 0;

    void Start()
    {
        control.OnForward += GoForward;
        control.OnLeft += GoLeft;
        control.OnBack += GoBack;
        control.OnRight += GoRight;
        control.OnAxisHorizontal += (x)=> HorizontalMove(x);
        control.OnAxisVertical += (x) => VerticalMove(x);

    }
       

    void HorizontalMove(float Axis)
    {
            transform.Rotate(0, Axis * sensitivityHor, 0);     
    }

    void VerticalMove(float Axis)
    {
        _rotationX -= Axis * sensitivityVert;
        _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
        float rotationY = transform.localEulerAngles.y;
        transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
    }

    void GoForward()
    {
        transform.Translate(new Vector3(.0f, .0f, Speed * Time.deltaTime), Space.Self);
    }

    void GoLeft()
    {
        transform.Translate(new Vector3(-Speed * Time.deltaTime, .0f, .0f),Space.Self);

    }

    void GoBack()
    {
        transform.Translate(new Vector3(.0f, .0f, -Speed * Time.deltaTime),Space.Self);
    }

    void GoRight()
    {
        transform.Translate(new Vector3( Speed * Time.deltaTime, .0f, .0f),Space.Self);
    }


}
