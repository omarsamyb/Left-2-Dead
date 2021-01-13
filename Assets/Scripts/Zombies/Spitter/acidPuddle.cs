using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class acidPuddle : MonoBehaviour
{
    bool takeDamage;
    void applyDamage()
    {
        if(takeDamage)
            PlayerController.instance.TakeDamage(10);
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
        if (other.gameObject.tag == "Player")
        {
            CancelInvoke();
            takeDamage = false;
        }
    }
    

}
