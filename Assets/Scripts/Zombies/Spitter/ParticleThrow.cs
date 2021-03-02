//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ParticleThrow : MonoBehaviour
//{
//    public Rigidbody rb;
//    public GameObject acidPuddle;
//    Transform player;
//    // Start is called before the first frame update
//    void Start()
//    {
//        rb = this.gameObject.GetComponent<Rigidbody>();
//        rb.useGravity = true;
//        Vector3 dir = Spitter.attackingPosition - transform.position;        
//        float distance = dir.magnitude;         
//        rb.AddForce(dir*25,ForceMode.Impulse);
//        rb.AddForce(transform.up*50, ForceMode.Impulse);
//        rb.AddTorque(new Vector3(10, 0, 10));
//    }
//    public void createPuddle()
//    {
//        Vector3 acidPuddlePos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
//        rb.velocity = Vector3.zero;
//        rb.angularVelocity = Vector3.zero;
//        GameObject acidPuddleSpare = Instantiate(acidPuddle, acidPuddlePos, Quaternion.Euler(90, 0, 0));

//        Destroy(gameObject);
//        Destroy(acidPuddleSpare, 10f);
//    }


//    void OnTriggerEnter(Collider other)
//    {
//        if (LayerMask.LayerToName(other.gameObject.layer) == "World" || LayerMask.LayerToName(other.gameObject.layer) == "Ground")
//            createPuddle();
//    }
//}
