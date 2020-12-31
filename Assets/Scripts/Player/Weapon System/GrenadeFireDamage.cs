using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeFireDamage : MonoBehaviour
{
    int radius = 5;
    void Start()
    {
        StartCoroutine(applyDamage());
    }
    IEnumerator applyDamage()
    {
        for (int i = 0; i < 5; i++)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, transform.forward);
            foreach (RaycastHit hit in hits)
            {
                GameObject cur = hit.transform.gameObject;
                //TODO: Damage zombies in range
                if(cur.tag=="Player")
                    print(cur.name+" got damaged!");
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
}
