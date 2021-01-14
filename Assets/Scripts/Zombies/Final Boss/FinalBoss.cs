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
    Transform handTransform;
    void Start()
    {
        handTransform = transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
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
        yield return new WaitForSeconds(50.0f/30.0f);
        meteor.gameObject.transform.localScale = new Vector3(0,0,0);
        meteor.gameObject.SetActive(true);
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.58)
        {
            if(meteor.gameObject.transform.localScale.x<10)
                meteor.gameObject.transform.localScale += new Vector3(1.0f/15.0f,1.0f/15.0f,1.0f/15.0f);
            meteor.transform.Rotate(1,1,1);
            meteor.transform.position = handTransform.position;
            yield return null;
        }
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.7)
        {
            meteor.transform.position = handTransform.position;
            yield return null;
        }
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
