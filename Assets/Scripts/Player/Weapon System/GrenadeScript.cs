﻿using System.Collections;
using UnityEngine;

public class GrenadeScript : MonoBehaviour
{
    public enum GrenadeType
    {
        molotov, pipe, stun, bile
    }
    public GrenadeType curNadeType;
    public GameObject Explosion;
    public GameObject Fire;
    Rigidbody rb;
    Transform mainCam;
    public float thrust = 20f;
    float fuseTime = 2;
    float explosionRadius = 5;
    public string _name;
    public int maxCapacity;
    private bool hitGround;

    void Start()
    {
        mainCam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        AudioManager.instance.SetSource("GrenadesSFX", GetComponent<AudioSource>());

        rb.velocity = mainCam.forward * thrust;
        rb.AddTorque(new Vector3(10, 0, 10));
    }

    void ExplodeMolotov()
    {
        GameObject boom = Instantiate(Explosion);
        GameObject fire = Instantiate(Fire);
        boom.transform.position = transform.position;
        fire.transform.position = transform.position;
        StartCoroutine(applyDamage(fire));
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
            r.enabled = false;
    }
    IEnumerator applyDamage(GameObject fire)
    {
        int radius = 5;
        for (int i = 0; i < 5; i++)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider cur in hits)
            {
                if(cur.tag=="Enemy")
                    cur.GetComponent<NormalInfected>().TakeDamage(25);
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject, 1);
    }
    void MakeNoisePipe()
    {
        //TODO: Make noise
        float attractRadius = explosionRadius * 2;
        Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius);
        foreach (Collider cur in hits)
        {
            if(cur.tag=="Enemy")
                cur.GetComponent<NormalInfected>().chase(transform);
        }
        StartCoroutine(ExplodePipe());
    }
    IEnumerator ExplodePipe()
    {
        yield return new WaitForSeconds(4f);
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            if(cur.tag=="Enemy")
                cur.GetComponent<NormalInfected>().TakeDamage(100);
        }
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 4);
    }
    IEnumerator ExplodeStun()
    {
        yield return new WaitForSeconds(0.2f);
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            if(cur.tag=="Enemy")
                cur.GetComponent<NormalInfected>().stun();
        }
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 5);
    }
    IEnumerator ExplodeBile()
    {
        GameObject boom = Instantiate(Explosion);
        for (int i = 0; i < 5; i++)
        {
            boom.transform.position = transform.position;
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider cur in hits)
            {
                if(cur.tag=="Enemy")
                    cur.GetComponent<NormalInfected>().Confuse();
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Untagged" && !hitGround) //TODO: Fix layer
        {
            hitGround = true;
            AudioManager.instance.PlayOneShot("GrenadesSFX");
            rb.velocity = new Vector3(0, 0, 0);
            if (curNadeType == GrenadeType.bile)
                StartCoroutine(ExplodeBile());
            else if (curNadeType == GrenadeType.molotov)
                ExplodeMolotov();
            else if (curNadeType == GrenadeType.pipe)
            {
                MakeNoisePipe();
            }
            else
                StartCoroutine(ExplodeStun());
        }
    }
}
