using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeScript : MonoBehaviour
{
    public enum GrenadeType{
	molotov,pipe,stun,bile
    } 
    public GrenadeType curNadeType;
    public GameObject Explosion;
    public GameObject Fire;
    Rigidbody rb;
    GameObject player;
    Transform mainCam;
    float thrust = 20f;
    float fuseTime = 2;
    bool thrown;
    float explosionRadius = 5;
    void Start() {
        thrown = false;
        player = GameObject.FindGameObjectWithTag("Player");
        mainCam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 5);
    }
    void Update()
    {
        if(!thrown && Input.GetAxis("Fire1")!=0)
        {
            Vector3 newPos = player.transform.position;
            newPos.y+=2f;
            transform.position = newPos;
            thrown=true;
            Vector3 rotation = mainCam.transform.forward;
            // Vector3 rotation = new Vector3(0.0f,0.0f,0.0f);
            rb.velocity = rotation*thrust;
            rb.AddTorque(new Vector3(10,0,10));
            // rb.AddForce(rotation*thrust);
            //Molotov and Bile explode on impact
            if(curNadeType == GrenadeType.pipe)
                Invoke("MakeNoisePipe", fuseTime);
            else if(curNadeType == GrenadeType.stun)
                Invoke("ExplodeStun", fuseTime);
        }
    }
    void ExplodeMolotov()
    {
        GameObject boom = Instantiate(Explosion);
        GameObject fire = Instantiate(Fire);
        boom.transform.position = transform.position;
        fire.transform.position = transform.position;
        StartCoroutine(applyDamage(fire));
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach(Renderer r in rs)
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
                //TODO: Damage zombies in range
                if(cur.tag=="Player")
                    print(cur.tag+" "+cur.name+" got damaged "+i);
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
    void MakeNoisePipe()
    {
        //TODO: Make noise
        float attractRadius = explosionRadius*2;
        Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius);
        foreach (Collider cur in hits)
        {
            //TODO: Attract zombies in range
            print("Pipe attracting: "+cur.name);
        }
        Invoke("ExplodePipe", 5);
    }
    void ExplodePipe()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            //TODO: Damage zombies in range
            print("Pipe Damaged: "+cur.name);
        }
        Destroy(this.gameObject);
    }
    void ExplodeStun()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            //TODO: Do whatever on objects in range of explosion
            print("Stunned: "+cur.name);
        }
        Destroy(this.gameObject);
    }
    IEnumerator ExplodeBile()
    {
        GameObject boom = Instantiate(Explosion);
        for(int i=0;i<5;i++)
        {
            boom.transform.position = transform.position;
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider cur in hits)
            {
                //TODO: Confuse Zombies in range of explosion
                if(cur.tag=="Player")
                print("Confused: "+cur.tag+" "+i);
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter(Collider other) 
    {
        print(other.gameObject.tag=="Untagged");
        if(thrown && other.gameObject.tag=="Untagged") //TODO: Fix layer
        {
            rb.velocity = new Vector3(0,0,0);
            if(curNadeType == GrenadeType.bile)
                StartCoroutine(ExplodeBile());
            else if (curNadeType == GrenadeType.molotov)
                ExplodeMolotov(); 
        }    
    }
}
