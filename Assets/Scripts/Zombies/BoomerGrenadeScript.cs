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
    public float thrust = 20f;
    Rigidbody rb;
    private bool hitGround;
    public GameObject Explosion;
    float explosionRadius = 2.5f;
    private LayerMask playerLayer;

    public GameObject thunder;
    private bool gotEffected;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = PlayerController.instance.player.transform;
        startPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        endPoint = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        direction = (endPoint - startPoint).normalized;
        rb = GetComponent<Rigidbody>();
  
        float dist = Vector3.Distance(playerTransform.position, transform.position);
        rb.velocity = new Vector3(direction.x, 0f, direction.z) * thrust * (Mathf.Min(dist,10.0f) / 10.0f);
        rb.AddTorque(new Vector3(10, 0, 10));
        playerLayer = 1 << LayerMask.NameToLayer("Player");

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Untagged" && !hitGround) //TODO: Fix layer
        {
            hitGround = true;
            if (!gotEffected)
            {
                rb.velocity = new Vector3(0, 0, 0);
                StartCoroutine(ExplodeBile());
            }
            else
            {
                GameObject boom = Instantiate(Explosion);
                boom.transform.position = transform.position;
            }
        }
        else if (other.gameObject.tag == "Player" && !hitGround)
        {
            rb.velocity = new Vector3(0, 0, 0);
            rb.mass = 1000;
            PlayerController.instance.BileVisionEffect();
            EffectOfBile();
        }
    }
    IEnumerator ExplodeBile()
    {
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        for (int i = 0; i < 16; i++)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, new Vector3(explosionRadius, 0.2f, explosionRadius), Quaternion.identity, playerLayer);
            if (hits.Length > 0)
            {
                EffectOfBile();
                yield break;
            }
            yield return new WaitForSeconds(0.25f);
        }
        Destroy(this.gameObject);
    }
    private void EffectOfBile()
    {
        if (!gotEffected)
        {
            HordeSpawner hordeSpawner = transform.gameObject.GetComponent<HordeSpawner>();
            hordeSpawner.bomberSpawnFlag = true;
            gotEffected = true;
            Thunder();
            Destroy(this.gameObject, 6f);
        }
    }
    public Vector3 CalculateBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
    public void Thunder()
    {
        GameObject thunderObject1 = Instantiate(thunder, transform.position, transform.rotation);
        thunderObject1.GetComponent<LightningBoltScript>().StartPosition = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z - 1f);
        thunderObject1.GetComponent<LightningBoltScript>().EndPosition = new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z - 1f);
        GameObject thunderObject2 = Instantiate(thunder, transform.position, transform.rotation);
        thunderObject2.GetComponent<LightningBoltScript>().StartPosition = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z + 1f);
        thunderObject2.GetComponent<LightningBoltScript>().EndPosition = new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z + 1f);
        GameObject thunderObject3 = Instantiate(thunder, transform.position, transform.rotation);
        thunderObject3.GetComponent<LightningBoltScript>().StartPosition = new Vector3(transform.position.x + 1f, transform.position.y - 1f, transform.position.z);
        thunderObject3.GetComponent<LightningBoltScript>().EndPosition = new Vector3(transform.position.x + 1f, transform.position.y + 10f, transform.position.z);
        GameObject thunderObject4 = Instantiate(thunder, transform.position, transform.rotation);
        thunderObject4.GetComponent<LightningBoltScript>().StartPosition = new Vector3(transform.position.x - 1f, transform.position.y - 1f, transform.position.z);
        thunderObject4.GetComponent<LightningBoltScript>().EndPosition = new Vector3(transform.position.x - 1f, transform.position.y + 10f, transform.position.z);
        Destroy(thunderObject1, 4.0f);
        Destroy(thunderObject2, 4.0f);
        Destroy(thunderObject3, 4.0f);
        Destroy(thunderObject4, 4.0f);
    }
}
