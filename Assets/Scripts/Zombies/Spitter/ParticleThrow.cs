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
        transform.parent = head.transform;
        rb = this.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        this.gameObject.SetActive(false);
        player = PlayerController.instance.player.transform;
    }
    public void createPuddle()
    {
        Vector3 acidPuddlePos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        GameObject acidPuddleSpare = Instantiate(acidPuddle, acidPuddlePos, Quaternion.Euler(90, 0, 0));

        this.gameObject.transform.position = head.transform.position;
        this.gameObject.SetActive(false);

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
        
        this.gameObject.SetActive(true);
        rb.useGravity = true;
        transform.rotation = head.transform.rotation;
        Vector3 dir = player.position - transform.position;
        dir.y=0;         
        float distance = dir.magnitude;         
        rb.AddForce(dir*30,ForceMode.Impulse);
        rb.AddTorque(new Vector3(10, 0, 10));
    }
}
