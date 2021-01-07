using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boomergrenade : MonoBehaviour
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
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        startPoint = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        endPoint = new Vector3(playerTransform.position.x, transform.position.y , playerTransform.position.z);
        direction = (endPoint - startPoint).normalized;

        rb = GetComponent<Rigidbody>();
        //AudioManager.instance.SetSource("GrenadesSFX", GetComponent<AudioSource>());

        rb.velocity = new Vector3(direction.x, 0f, direction.z)* thrust;
        rb.AddTorque(new Vector3(10, 0, 10));
        Destroy(gameObject, 10.0f);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !isSpawned)
        {
            HordeSpawner x = transform.parent.gameObject.GetComponent<HordeSpawner>();
            x.bomberSpawnFlag = true;
            Destroy(gameObject);
            isSpawned = true;
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
