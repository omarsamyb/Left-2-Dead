using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeScript : MonoBehaviour
{
    public enum GrenadeType{
	molotov,pipe,stun,bile
    } 
    public GrenadeType curNadeType;
    public GameObject molotovExplosion;
    public GameObject molotovFire;

    public GameObject stunExplosion;
    public GameObject pipeExplosion;
    public GameObject bileExplosion;
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
        GameObject boom = Instantiate(molotovExplosion);
        GameObject fire = Instantiate(molotovFire);
        boom.transform.position = transform.position;
        fire.transform.position = transform.position;
        StartCoroutine(applyDamage());
        Destroy(this.gameObject);
    }
    IEnumerator applyDamage()
    {
        print("IN");
        // int radius = 5;
        yield return new WaitForSeconds(0.1f);
        // for (int i = 0; i < 5; i++)
        // {
        //     // RaycastHit[] hits = Physics.SphereCastAll(fire.transform.position, radius, fire.transform.forward);
        //     // RaycastHit[] hits = Physics.SphereCastAll(fire.transform.position, radius, fire.transform.forward);
        //     // foreach (RaycastHit hit in hits)
        //     // {
        //     //     GameObject cur = hit.transform.gameObject;
        //     //     //TODO: Damage zombies in range
        //     //     // if(cur.tag=="Player")
        //     //     print(cur.tag+" "+cur.name+" got damaged!");
        //     // }
        //     // yield return new WaitForSeconds(1);
        //     // yield return null;
        //     print(i);
        // }
        print("OUT");
        // Destroy(fire.gameObject);
    }
    void MakeNoisePipe()
    {
        //TODO: Make noise
        float attractRadius = explosionRadius*2;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, attractRadius, transform.forward);
        foreach (RaycastHit hit in hits)
        {
            GameObject cur = hit.transform.gameObject;
            //TODO: Attract zombies in range
            print("Pipe attracting: "+cur.name);
        }
        Invoke("ExplodePipe", 5);
    }
    void ExplodePipe()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionRadius, transform.forward);
        GameObject boom = Instantiate(pipeExplosion);
        boom.transform.position = transform.position;
        foreach (RaycastHit hit in hits)
        {
            GameObject cur = hit.transform.gameObject;
            //TODO: Damage zombies in range
            print("Pipe Damaged: "+cur.name);
        }
        Destroy(this.gameObject);
    }
    void ExplodeStun()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionRadius, transform.forward);
        GameObject boom = Instantiate(molotovExplosion);
        boom.transform.position = transform.position;
        foreach (RaycastHit hit in hits)
        {
            GameObject cur = hit.transform.gameObject;
            //TODO: Do whatever on objects in range of explosion
            print("Stunned: "+cur.name);
        }
        Destroy(this.gameObject);
    }
    void ExplodeBile()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionRadius, transform.forward);
        GameObject boom = Instantiate(bileExplosion);
        boom.transform.position = transform.position;
        foreach (RaycastHit hit in hits)
        {
            GameObject cur = hit.transform.gameObject;
            //TODO: Confuse Zombies in range of explosion
            print("Confused: "+cur.name);
        }
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter(Collider other) 
    {
        if(thrown && other.gameObject.tag=="Untagged") //TODO: Fix layer
        {
            if(curNadeType == GrenadeType.bile)
                ExplodeBile();
            else if (curNadeType == GrenadeType.molotov)
                ExplodeMolotov();
        }    
    }
}
