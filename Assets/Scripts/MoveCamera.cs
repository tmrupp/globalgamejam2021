using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    Camera mc;
    [SerializeField] float speed;

    void Start()
    {
        mc = Camera.main;
        speed = 1f;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        if(Input.GetKeyDown(KeyCode.W))
        {
            move += Vector3.up;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            move += Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            move += Vector3.down;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            move += Vector3.right;
        }

        mc.transform.position += move;
    }
}
