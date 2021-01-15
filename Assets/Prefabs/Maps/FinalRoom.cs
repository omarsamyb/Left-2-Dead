using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalRoom : MonoBehaviour
{
    public GameObject barrier;
    public GameObject finalBoss;
    bool spawned = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&&!spawned)
        {
            spawned = true;
            Destroy(barrier);
            Instantiate(finalBoss);
            GameManager.instance.timerIsRunning = false;
            GameManager.instance.failedRescue = false;
        }
    }
}
