using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    public AudioSource source;
    private State currentState;
    private EnemyContoller enemyController;

    public AudioClip[] idleClips;
    public AudioClip[] chasingClips;
    public AudioClip[] damagedClips;
    public AudioClip[] deadClips;
    public AudioClip[] attackClips;
    private int idleIndex;
    private int chasingIndex;
    private int damagedIndex;
    private int deadIndex;
    private int attackIndex;

    void Start()
    {
        enemyController = GetComponent<EnemyContoller>();
    }

    void Update()
    {
        currentState = enemyController.currentState;
        Idle();
        Chasing();
    }

    private void Idle()
    {
        if (!source.isPlaying)
        {
            if (currentState == State.patrol || currentState == State.idle || currentState == State.coolDown)
            {
                source.clip = idleClips[idleIndex];
                source.Play();
                idleIndex = (idleIndex + 1) % idleClips.Length;
            }
        }
    }
    private void Chasing()
    {
        if (!source.isPlaying)
        {
            if (currentState == State.hear || currentState == State.chasing)
            {
                chasingIndex = Random.Range(0, chasingClips.Length);
                source.clip = chasingClips[chasingIndex];
                source.Play();
                chasingIndex = (chasingIndex + 1) % chasingClips.Length;
            }
        }
    }
    public void Attack(int index)
    {
        if (index == -1)
        {
            source.clip = attackClips[attackIndex];
            source.Play();
            attackIndex = (attackIndex + 1) % attackClips.Length;
        }
        else if(index != -1)
        {
            source.clip = attackClips[index];
            source.Play();
        }
    }
    public void Damaged()
    {
        if (!source.isPlaying || currentState == State.chasing || currentState == State.idle || currentState == State.patrol)
        {
            source.clip = damagedClips[damagedIndex];
            source.Play();
            damagedIndex = (damagedIndex + 1) % damagedClips.Length;
        }
    }
    public void Dead()
    {
        deadIndex = Random.Range(0, deadClips.Length);
        source.clip = deadClips[deadIndex];
        source.Play();
    }
}
