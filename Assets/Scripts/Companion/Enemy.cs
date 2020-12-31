using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
        {
         if(col.gameObject.tag == "Bullet"){
                Debug.Log("Hit");
                Destroy(col.gameObject);
            }
        }
}
