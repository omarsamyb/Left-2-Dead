using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] audioClips;
    public Animator animator;
    public GameObject meteor;
    bool canAttack;
    public int health = 10000;
    float coolDownTime = 20f;
    Transform handTransform;
    bool canPlayIdleSound;
    public HealthBar healthBar;
    public GameObject healthBarUI;
    void Start()
    {
        healthBar.SetMaxHealth(health);
        handTransform = transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        source = GetComponent<AudioSource>();
        StartCoroutine(PlayWakeUpSound());
        StartCoroutine(coolDown(5));
    }
    void Update()
    {
        if(canAttack && health>0)
        {
            canPlayIdleSound = false;
            if(Random.Range(0.0f, 1.0f)>0.5)
                StartCoroutine(earthquake());
            else 
                StartCoroutine(spawnZombies());
            // StartCoroutine(spawnZombies());
        }
        if(canPlayIdleSound && health>0)
        {
            source.clip = audioClips[Random.Range(5, 8)];
            source.Play();
            canPlayIdleSound = false;
        }
    }
    IEnumerator spawnZombies()
    {
        source.clip = audioClips[3];
        source.Play();
        canAttack = false;
        animator.SetTrigger("Throw");
        yield return new WaitForSeconds(50.0f/30.0f);
        if(health<=0) yield break;
        meteor.gameObject.transform.localScale = new Vector3(0,0,0);
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.58)
        {
            if(meteor.gameObject.transform.localScale.x<10)
                meteor.gameObject.transform.localScale += new Vector3(1.0f/15.0f,1.0f/15.0f,1.0f/15.0f);
            meteor.transform.Rotate(1,1,1);
            meteor.transform.position = handTransform.position;
            yield return null;
            if(health<=0) yield break;
        }
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime<0.7)
        {
            meteor.transform.position = handTransform.position;
            yield return null;
            if(health<=0) yield break;
        }
        meteor.GetComponent<MeteorScript>().ThrowMeteor();
        StartCoroutine(canPlayIdle());
        StartCoroutine(coolDown(coolDownTime));
    }
    IEnumerator earthquake()
    {
        source.clip = audioClips[1];
        source.Play();
        canAttack = false;
        animator.SetTrigger("Earthquake");
        yield return new WaitForSeconds(60.0f/30.0f);
        if(health<=0) yield break;
        source.clip = audioClips[2];
        source.Play();
        for(int i=0;i<10;i++)
        {
            if(PlayerController.instance.isGrounded)
            {
                PlayerController.instance.TakeDamage(50);
                break;
            }
            yield return null;
            if(health<=0) yield break;
        }
        StartCoroutine(canPlayIdle());
        StartCoroutine(coolDown(coolDownTime));
    }
    IEnumerator coolDown(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }
    public void TakeDamage(int damage)
    {
        healthBar.SetHealth(health);
        health-=damage;
        if(health<=0)
            Die();
    }
    void Die()
    {
        Destroy(healthBarUI);
        source.clip = audioClips[4];
        source.Play();
        animator.SetTrigger("Dead");
    }
    IEnumerator PlayWakeUpSound()
    {
        yield return new WaitForSeconds(1.1f);
        if(health>0)
        {
            source.clip = audioClips[0];
            source.Play();
        }
    }
    IEnumerator canPlayIdle()
    {
        yield return new WaitForSeconds(5f);
        if(health>0)
            canPlayIdleSound = true;
    }
}
