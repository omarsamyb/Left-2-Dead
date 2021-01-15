using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfRescue : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            GameManager.instance.timerIsRunning = false;
            GameManager.instance.failedRescue = false;
        }
    }
}
