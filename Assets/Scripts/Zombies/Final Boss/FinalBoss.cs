using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    public Animator animator;
    public GameObject meteor;
    bool canAttack;
    public int health = 10000;
    float coolDownTime = 5f;
    void Start()
    {
        StartCoroutine(coolDown());
    }
    void Update()
    {
        if(canAttack && health>0)
        {
            // if(Random.Range(0.0f, 1.0f)>0.5)
            //     StartCoroutine(earthquake());
            // else 
            //     StartCoroutine(spawnZombies());
            StartCoroutine(spawnZombies());
        }
    }
    IEnumerator spawnZombies()
    {
        canAttack = false;
        animator.SetTrigger("Throw");
        yield return new WaitForSeconds(160.0f/30.0f);
        meteor.GetComponent<MeteorScript>().ThrowMeteor();
        StartCoroutine(coolDown());
    }
    IEnumerator earthquake()
    {
        canAttack = false;
        animator.SetTrigger("Earthquake");
        yield return new WaitForSeconds(55.0f/30.0f);
        for(int i=0;i<10;i++)
        {
            if(PlayerController.instance.isGrounded)
            {
                PlayerController.instance.TakeDamage(50);
                break;
            }
            yield return 0;
        }
        //If player is on ground 
            //Take Damage
        StartCoroutine(coolDown());
    }
    IEnumerator coolDown()
    {
        yield return new WaitForSeconds(coolDownTime);
        canAttack = true;
    }
    public void TakeDamage(int damage)
    {
        health-=damage;
        if(health<=0)
            Die();
    }
    void Die()
    {
        animator.SetTrigger("Dead");
    }
}
