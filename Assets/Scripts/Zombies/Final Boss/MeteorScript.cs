using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorScript : MonoBehaviour
{
    Rigidbody rb;
    public GameObject horde;
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }
    public void ThrowMeteor()
    {
        gameObject.SetActive(true);
        rb.useGravity = true;
        Vector3 dir = PlayerController.instance.player.transform.position - transform.position;
        // dir.y=0;
        float distance = dir.magnitude;
        rb.AddForce(dir,ForceMode.Impulse);
        // rb.AddTorque(new Vector3(10, 0, 10));
    }
    
    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        if(LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            PlayerController.instance.TakeDamage(50);
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == "World")
        {
            GameObject myHorde = Instantiate(horde);
            myHorde.transform.position = transform.position;
        }
    }
}
