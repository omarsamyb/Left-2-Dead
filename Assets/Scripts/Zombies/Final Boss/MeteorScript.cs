using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorScript : MonoBehaviour
{
    Rigidbody rb;
    public AudioSource source;
    public AudioClip audioClip;
    public GameObject horde;
    public GameObject dustExplosion;
    bool canSpawnZombies;
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        transform.localScale = new Vector3(0,0,0);
    }
    public void ThrowMeteor()
    {
        canSpawnZombies = true;
        rb.useGravity = true;
        Vector3 dir = PlayerController.instance.player.transform.position - transform.position;
        dir = Vector3.Normalize(dir);
        float dist = Vector3.Distance(PlayerController.instance.player.transform.position, transform.position);
        rb.velocity = new Vector3(dir.x, 0f, dir.z) * dist/2.7f;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="FinalBoss" || !canSpawnZombies)
            return;
        transform.localScale = new Vector3(0,0,0);
        if(Vector3.Distance(PlayerController.instance.player.transform.position, transform.position)<7)
            PlayerController.instance.TakeDamage(50);
        else
            Instantiate(horde, transform.position, Quaternion.identity);
        source.clip = audioClip;
        source.Play();
        Instantiate(dustExplosion, transform.position, Quaternion.identity);
        canSpawnZombies = false;
    }
}
