using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class acidPuddle : MonoBehaviour
{
    bool takeDamage;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {


    }
    void applyDamage()
    {
        //Debug.Log("I Got A Player:))))");
        player.GetComponent<PlayerController>().TakeDamage(10);
        Debug.Log(player.GetComponent<PlayerController>().health);
        //Debug.Log(player.GetComponent<PlayerController>().health);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            InvokeRepeating(nameof(applyDamage), 0f, 1f);
            takeDamage = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log("Exiting Damage!!!!");
            CancelInvoke();
            takeDamage = false;

        }
    }
    

}
