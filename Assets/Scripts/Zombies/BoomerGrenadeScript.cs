using DigitalRuby.LightningBolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerGrenadeScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform playerTransform;
    Vector3 startPoint;
    Vector3 endPoint;
    public float timer = 0;
    private Vector3 direction;
    private bool isSpawned = false;
    public float thrust = 20f;
    Rigidbody rb;

    public GameObject thunder;
    Transform parentTransform;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = PlayerController.instance.player.transform;
        startPoint = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        endPoint = new Vector3(playerTransform.position.x, transform.position.y , playerTransform.position.z);
        direction = (endPoint - startPoint).normalized;
        //   parentTransform = transform.parent.transform;
        parentTransform = playerTransform;
        rb = GetComponent<Rigidbody>();
        float dist = Vector3.Distance(playerTransform.position, transform.position);
        rb.velocity = new Vector3(direction.x, 0f, direction.z)* thrust * (dist/10.0f);
        rb.AddTorque(new Vector3(10, 0, 10));
        Destroy(gameObject, 10.0f);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !isSpawned)
        {
            isSpawned = true;
            GameObject thunderObject1= Instantiate(thunder, parentTransform.position, parentTransform.rotation);
            thunderObject1.GetComponent<LightningBoltScript>().StartPosition= new Vector3(parentTransform.position.x, parentTransform.position.y -1f, parentTransform.position.z - 1f);
            thunderObject1.GetComponent<LightningBoltScript>().EndPosition=new Vector3(parentTransform.position.x, parentTransform.position.y+10f, parentTransform.position.z - 1f);
            GameObject thunderObject2 = Instantiate(thunder, parentTransform.position, parentTransform.rotation);
            thunderObject2.GetComponent<LightningBoltScript>().StartPosition = new Vector3(parentTransform.position.x, parentTransform.position.y - 1f, parentTransform.position.z + 1f);
            thunderObject2.GetComponent<LightningBoltScript>().EndPosition = new Vector3(parentTransform.position.x, parentTransform.position.y + 10f, parentTransform.position.z + 1f);
            GameObject thunderObject3 = Instantiate(thunder, parentTransform.position, parentTransform.rotation);
            thunderObject3.GetComponent<LightningBoltScript>().StartPosition = new Vector3(parentTransform.position.x + 1f, parentTransform.position.y - 1f, parentTransform.position.z);
            thunderObject3.GetComponent<LightningBoltScript>().EndPosition = new Vector3(parentTransform.position.x + 1f, parentTransform.position.y + 10f, parentTransform.position.z);
            GameObject thunderObject4 = Instantiate(thunder, parentTransform.position, parentTransform.rotation);
            thunderObject4.GetComponent<LightningBoltScript>().StartPosition = new Vector3(parentTransform.position.x-1f, parentTransform.position.y - 1f, parentTransform.position.z);
            thunderObject4.GetComponent<LightningBoltScript>().EndPosition = new Vector3(parentTransform.position.x-1f, parentTransform.position.y + 10f, parentTransform.position.z);
            Destroy(thunderObject1, 4.0f);
            Destroy(thunderObject2, 4.0f);
            Destroy(thunderObject3, 4.0f);
            Destroy(thunderObject4, 4.0f);

            // need to call vision effect
            PlayerController.instance.BileVisionEffect();

            HordeSpawner hordeSpawner = transform.gameObject.GetComponent<HordeSpawner>();
            hordeSpawner.bomberSpawnFlag = true;

        }
    }
    public Vector3 CalculateBezierCurve(float t , Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0 )+ (2 * u*t* p1) +( tt * p2);
    }
}
