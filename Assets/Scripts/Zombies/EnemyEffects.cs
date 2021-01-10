using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    private AudioSource audioSource;
    private State currentState;
    private EnemyContoller enemyController;
    public AudioClip[] idleClips, attackAndChasingClips;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        enemyController = GetComponent<EnemyContoller>();

    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
            return;
        currentState = enemyController.currentState;
        if (currentState == State.patrol || currentState == State.idle || currentState == State.coolDown)
        {
            audioSource.clip = idleClips[Random.Range(0, idleClips.Length)];
            audioSource.Play();
        }
        else if (currentState == State.attack || currentState == State.hear || currentState == State.chasing)
        {
            audioSource.clip = attackAndChasingClips[Random.Range(0, attackAndChasingClips.Length)];
            audioSource.Play();
        }
    }
}
