using System.Collections;
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
    float explosionRadius = 5;
    public string _name;
    public int maxCapacity;
    private bool hitGround;
    private LayerMask enemyLayer;

    void Start()
    {
        mainCam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        AudioManager.instance.SetSource("GrenadesSFX", GetComponent<AudioSource>());

        rb.velocity = mainCam.forward * thrust;
        rb.AddTorque(new Vector3(10, 0, 10));

        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    void ExplodeMolotov()
    {
        GameObject boom = Instantiate(Explosion);
        GameObject fire = Instantiate(Fire);
        boom.transform.position = transform.position;
        fire.transform.position = transform.position;
        StartCoroutine(applyDamage());
        transform.localScale = Vector3.zero;
    }
    IEnumerator applyDamage()
    {
        int radius = 5;
        for (int i = 0; i < 5; i++)
        {
            Collider[] hits = Physics.OverlapBox(new Vector3(transform.position.x, 1f, transform.position.z), new Vector3(radius, 1f, radius), Quaternion.identity, enemyLayer);
            foreach (Collider cur in hits)
            {
                cur.GetComponent<EnemyContoller>().TakeDamage(25);
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject, 1);
    }
    void MakeNoisePipe()
    {
        float attractRadius = explosionRadius * 2;
        Collider[] hits = Physics.OverlapBox(new Vector3(transform.position.x, 1f, transform.position.z), new Vector3(attractRadius, 1f, attractRadius), Quaternion.identity, enemyLayer);
        foreach (Collider cur in hits)
        {
            cur.GetComponent<EnemyContoller>().chase(transform);
        }
        StartCoroutine(ExplodePipe());
    }
    IEnumerator ExplodePipe()
    {
        yield return new WaitForSeconds(4f);
        Collider[] hits = Physics.OverlapBox(new Vector3(transform.position.x, 1f, transform.position.z), new Vector3(explosionRadius, 1f, explosionRadius), Quaternion.identity, enemyLayer);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            cur.GetComponent<EnemyContoller>().TakeDamage(100);
        }
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 4);
    }
    IEnumerator ExplodeStun()
    {
        yield return new WaitForSeconds(0.2f);
        Collider[] hits = Physics.OverlapBox(new Vector3(transform.position.x, 1f, transform.position.z), new Vector3(explosionRadius, 1f, explosionRadius), Quaternion.identity, enemyLayer);
        GameObject boom = Instantiate(Explosion);
        boom.transform.position = transform.position;
        foreach (Collider cur in hits)
        {
            cur.GetComponent<EnemyContoller>().stun();
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
            Collider[] hits = Physics.OverlapBox(new Vector3(transform.position.x, 1f, transform.position.z), new Vector3(explosionRadius, 1f, explosionRadius), Quaternion.identity, enemyLayer);
            foreach (Collider cur in hits)
            {
                cur.GetComponent<EnemyContoller>().Confuse();
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
