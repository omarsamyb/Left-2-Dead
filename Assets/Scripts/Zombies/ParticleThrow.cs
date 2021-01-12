using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleThrow : MonoBehaviour
{
    public GameObject head;
    public Rigidbody rb;
    public GameObject acidPuddle;
    Transform player;
    // Start is called before the first frame update
    void Start()
    {

        //sc = FindObjectOfType<SpitController>();
        //head = GameObject.FindGameObjectWithTag("SpitterHead");
        transform.parent = head.transform;
        rb = this.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        this.gameObject.active = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void createPuddle()
    {
        //GameObject acidSpare = Instantiate(this.gameObject, head.transform.position, Quaternion.identity);


        Vector3 acidPuddlePos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        //Destroy(this.gameObject, 1f);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        GameObject acidPuddleSpare = Instantiate(acidPuddle, acidPuddlePos, Quaternion.identity);
        //head = GameObject.FindGameObjectWithTag("SpitterHead");

        this.gameObject.transform.position = head.transform.position;
        this.gameObject.active = false;

        Destroy(acidPuddleSpare, 10f);


    }


    void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "World" || LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            if (LayerMask.LayerToName(other.gameObject.layer) == "World")
                createPuddle();

        }

    }

    public void ReleaseMe()
    {
        
        this.gameObject.active = true;
        transform.parent = null;

        rb.useGravity = true;
        transform.rotation = head.transform.rotation;

        rb.AddForce(transform.forward * 180f, ForceMode.Impulse);
        rb.AddForce(transform.up * 50f, ForceMode.Impulse);


    }

   


}
