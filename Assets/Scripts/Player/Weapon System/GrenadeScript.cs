using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    public float thrust = 10f;
    float explosionRadius = 5;
    public string _name;
    public int maxCapacity;
    private bool hitGround;
    private LayerMask enemyLayer;
    private Rage rage;
    InventoryObject ingredientInventory;
    public Light lightSource;
    private bool exploaded;

    void Start()
    {
        mainCam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        AudioManager.instance.SetSource("GrenadesSFX", GetComponent<AudioSource>());

        rb.AddForce(mainCam.forward * thrust, ForceMode.VelocityChange);
        rb.AddTorque(new Vector3(10f, 0f, 10f), ForceMode.VelocityChange);

        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        rage = PlayerController.instance.gameObject.GetComponent<Rage>();
        ingredientInventory = PlayerController.instance.transform.GetComponent<PlayerInventory>().ingredientInventory;
    }

    void ExplodeMolotov()
    {
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(transform.position, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            Instantiate(Explosion, navMeshHit.position, Explosion.transform.rotation);
            Instantiate(Fire, navMeshHit.position, Fire.transform.rotation);
            StartCoroutine(applyDamage());
            transform.localScale = Vector3.zero;
        }    
    }
    IEnumerator applyDamage()
    {
        int radius = 5;
        Vector3 position = transform.position;
        for (int i = 0; i < 5; i++)
        {
            Collider[] hits = Physics.OverlapBox(position, new Vector3(radius, 0.4f, radius), Quaternion.identity, enemyLayer);
            foreach (Collider cur in hits)
            {
                InfectedController enemy = cur.GetComponent<InfectedController>();
                enemy.TakeDamage(25, 2);
                if (enemy.health <= 0)
                {
                    rage.UpdateRage(enemy.transform.tag);
                    CompanionController.instance.killCounter++;
                    if (enemy.transform.tag[0] == 'S')
                    {
                        ingredientInventory.container[1].addAmount(1);
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject, 1);
    }
    void MakeNoisePipe()
    {
        float attractRadius = explosionRadius * 2;
        Collider[] hits = Physics.OverlapBox(transform.position, new Vector3(attractRadius, 0.4f, attractRadius), Quaternion.identity, enemyLayer);
        foreach (Collider cur in hits)
        {
            cur.GetComponent<InfectedController>().Pipe(transform);
        }
        StartCoroutine(ExplodePipe());
        StartCoroutine(LightFlicker());
    }
    IEnumerator ExplodePipe()
    {
        Vector3 position = transform.position;
        yield return new WaitForSeconds(4f);
        Collider[] hits = Physics.OverlapBox(position, new Vector3(explosionRadius, 0.4f, explosionRadius), Quaternion.identity, enemyLayer);
        Instantiate(Explosion, transform.position, Quaternion.identity);
        foreach (Collider cur in hits)
        {
            InfectedController enemy = cur.GetComponent<InfectedController>();
            Rigidbody[] rigidBodies = cur.GetComponents<Rigidbody>();
            enemy.TakeDamage(100, 1);
            foreach(Rigidbody rb in rigidBodies)
            {
                if (rb != null)
                    rb.AddExplosionForce(1000.0f, transform.position, explosionRadius, 3.0f);
            }          
            if (enemy.health <= 0)
            {
                rage.UpdateRage(enemy.transform.tag);
                CompanionController.instance.killCounter++;
                if (enemy.transform.tag[0] == 'S')
                {
                    ingredientInventory.container[1].addAmount(1);
                }
            }
        }
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 4);
    }
    IEnumerator LightFlicker()
    {
        while (!exploaded)
        {
            lightSource.enabled = !lightSource.isActiveAndEnabled;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator ExplodeStun()
    {
        Vector3 position = transform.position;
        yield return new WaitForSeconds(0.2f);
        Collider[] hits = Physics.OverlapBox(position, new Vector3(explosionRadius, 0.4f, explosionRadius), Quaternion.identity, enemyLayer);
        Instantiate(Explosion, transform.position, Explosion.transform.rotation);
        foreach (Collider cur in hits)
        {
            cur.GetComponent<InfectedController>().Stun();
        }
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 5);
    }
    private void ExplodeBile()
    {
        Instantiate(Explosion, transform.position, Explosion.transform.rotation);
        Collider[] hits = Physics.OverlapBox(transform.position, new Vector3(explosionRadius, 0.4f, explosionRadius), Quaternion.identity, enemyLayer);
        foreach (Collider cur in hits)
        {
            cur.GetComponent<InfectedController>().Bile();
        }
        Destroy(gameObject, 8f);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.layer == LayerMask.NameToLayer("Ground") || curNadeType == GrenadeType.molotov) && !hitGround)
        {
            hitGround = true;
            AudioManager.instance.PlayOneShot("GrenadesSFX");
            rb.velocity = new Vector3(0, 0, 0);
            if (curNadeType == GrenadeType.bile)
                ExplodeBile();
            else if (curNadeType == GrenadeType.molotov)
                ExplodeMolotov();
            else if (curNadeType == GrenadeType.pipe)
                MakeNoisePipe();
            else
                StartCoroutine(ExplodeStun());
        }
    }
}
