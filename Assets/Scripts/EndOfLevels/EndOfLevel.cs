using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfLevel : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            GameObject[] normalEnemy = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] specialEnemy = GameObject.FindGameObjectsWithTag("SpecialEnemy");
            if(normalEnemy.Length == 0 && specialEnemy.Length == 0){
                GameManager.instance.LoadNextLevel();
            }
        }
    }
}
