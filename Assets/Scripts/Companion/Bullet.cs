using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float moveSpeed = 8f;
    Rigidbody rb;
    Vector3 moveDirection;

    // Start is called before the first frame update
    void Start()
    {   
        if(companionController.instance.chosenEnemy != null){
        rb = GetComponent<Rigidbody>();
        moveDirection = (companionController.instance.chosenEnemy.transform.position - transform.position).normalized * moveSpeed;
        rb.velocity = new Vector3(moveDirection.x,moveDirection.y,moveDirection.z);
        Destroy(gameObject,3.0f);
        }
    }

}
