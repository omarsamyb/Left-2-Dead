using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour
{
    public float amount = 2f;
    public float speed = 1f;
    public Vector3 axis = Vector3.zero;
    private float amountTracker;
    private bool direction;

    void Start()
    {
        amountTracker = 0f;
    }

    void Update()
    {
        if(amountTracker < amount)
        {
            amountTracker += speed * Time.deltaTime;
            if (direction)
                transform.Translate(-axis.x * speed * Time.deltaTime, -axis.y * speed * Time.deltaTime, -axis.z * speed * Time.deltaTime);
            else
                transform.Translate(axis.x * speed * Time.deltaTime, axis.y * speed * Time.deltaTime, axis.z * speed * Time.deltaTime);
        }
        else
        {
            amountTracker = 0f;
            direction = !direction;
        }
    }
}
