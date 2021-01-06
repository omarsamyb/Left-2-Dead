using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boomergrenade : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform playerTransform;
    Vector3 startPoint;
    Vector3 endPoint;
    Vector3 midPoint;
    private Vector3[] positions = new Vector3[50];
    public float timer = 0;
    private Vector3 direction;
    private bool isSpawned = false;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        startPoint = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        endPoint = new Vector3(playerTransform.position.x, transform.position.y , playerTransform.position.z);
        direction = (endPoint - startPoint).normalized;
        Destroy(gameObject, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = new Vector3(0f, 0f, Mathf.Abs(direction.z));
        transform.Translate(forward * 10 *Time.deltaTime);

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
