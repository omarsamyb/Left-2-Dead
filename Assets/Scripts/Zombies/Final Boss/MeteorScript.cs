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
        rb.useGravity = true;
        Vector3 dir = PlayerController.instance.player.transform.position - transform.position;
        dir = Vector3.Normalize(dir);
        float dist = Vector3.Distance(PlayerController.instance.player.transform.position, transform.position);
        rb.velocity = new Vector3(dir.x, 0f, dir.z) * dist/2.7f;
    }
    
    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if(other.gameObject.tag!="FinalBoss")
            gameObject.SetActive(false);
        if(LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            PlayerController.instance.TakeDamage(50);
        }
        else if (LayerMask.LayerToName(other.gameObject.layer) == "World")
        {
            GameObject myHorde = Instantiate(horde);
            myHorde.transform.position = transform.position;
            print(myHorde.transform.position);
        }
    }
}
