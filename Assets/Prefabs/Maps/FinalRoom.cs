using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalRoom : MonoBehaviour
{
    public GameObject barrier;
    public GameObject finalBoss;
    public GameObject boom;
    bool spawned = false;
    void Start(){
        boom.GetComponent<ParticleSystem>().Stop();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&&!spawned)
        {
            spawned = true;
            StartCoroutine(playPS());
            Destroy(barrier);
            Instantiate(finalBoss);
            GameManager.instance.timerIsRunning = false;
            GameManager.instance.failedRescue = false;
        }
    }
    IEnumerator playPS(){
        boom.SetActive(true);
        boom.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(1.5f);
        boom.GetComponent<ParticleSystem>().Stop();
        boom.SetActive(false);
    }
}
