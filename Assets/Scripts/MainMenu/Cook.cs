using System.Collections;
using UnityEngine;

public class Cook : MonoBehaviour
{
    public float speed = 6f;

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
