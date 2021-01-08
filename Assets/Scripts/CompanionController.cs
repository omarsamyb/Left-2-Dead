using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof (NavMeshAgent))]
public class CompanionController : MonoBehaviour
{
    public string weaponName;
    public int maxClips = 3;
    public GameObject muzzelSpawn;

    private GameObject[] muzzelFlash;
    private AudioClip shootSFX;
    private GameObject bloodEffect;
    private GameObject wallDecalEffect;
    private GunStyles style;
    private int amountOfBulletsPerLoad;
    private int bulletsIHave;
    private int currentClips;
    private float roundsPerSecond;
    private int damage;
    private float waitTillNextFire;
    private RaycastHit hitInfo;
    private AudioSource fireSource;
    private TextMesh HUD_companion;

    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private Transform player;
    private PlayerController playerController;
    private Animator animator;
    private EnemyContoller normalEnemy;
    private EnemyContoller specialEnemy;
    private EnemyContoller chosenEnemy;
    Collider[] hits;
    LayerMask enemyLayer;
    LayerMask shootingLayer;
    private float range;

    void Start()
    {
        GunScript weapon = ((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + weaponName + ".prefab", typeof(GameObject))).GetComponent<GunScript>();
        muzzelFlash = weapon.muzzelFlash;
        shootSFX = weapon.shootSFX;
        bloodEffect = weapon.bloodEffect;
        wallDecalEffect = weapon.wallDecalEffect;
        style = weapon.currentStyle;
        amountOfBulletsPerLoad = (int)weapon.amountOfBulletsPerLoad;
        roundsPerSecond = weapon.roundsPerSecond;
        damage = (int)weapon.damage;
        fireSource = GetComponent<AudioSource>();
        fireSource.clip = shootSFX;
        currentClips = 1;
        bulletsIHave = amountOfBulletsPerLoad;

        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        animator = GetComponent<Animator>();
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        shootingLayer = enemyLayer | (1 << LayerMask.NameToLayer("World"));
        player = PlayerController.instance.player.transform;
        playerController = player.GetComponent<PlayerController>();
        range = 60f;
    }

    void Update()
    {
        Shooting();
        Movement();
    }
    private void Movement()
    {
        if (Vector3.Distance(player.position, transform.position) > agent.stoppingDistance + agent.radius * 2)
        {
            animator.SetBool("isMoving", true);
            obstacle.enabled = false;
            agent.enabled = true;
            agent.SetDestination(player.position);
        }

        if (!agent.pathPending && agent.enabled)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + agent.radius * 2)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude <= 1f)
                {
                    animator.SetBool("isMoving", false);
                    agent.ResetPath();
                    agent.enabled = false;
                    obstacle.enabled = true;
                }
            }
        }
    }
    private void Shooting()
    {
        if (style == GunStyles.nonautomatic)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                ShootWeapon();
        }
        else if (style == GunStyles.automatic)
        {
            if (Input.GetKey(KeyCode.Q))
                ShootWeapon();
        }
        waitTillNextFire -= roundsPerSecond * Time.deltaTime;
    }
    private void ShootWeapon()
    {
        if (waitTillNextFire <= 0 && bulletsIHave > 0)
        {
            int randomNumberForMuzzelFlash = Random.Range(0, 5);
            StartCoroutine(Bullet());
            Instantiate(muzzelFlash[randomNumberForMuzzelFlash], muzzelSpawn.transform.position, muzzelSpawn.transform.rotation * Quaternion.Euler(0, 0, 90), muzzelSpawn.transform);
            fireSource.Play();
            waitTillNextFire = 1;
            if(!GameManager.instance.inRageMode)
                bulletsIHave--;
            if (bulletsIHave == 0)
            {
                currentClips--;
                if (currentClips != 0)
                    bulletsIHave += amountOfBulletsPerLoad;
            }
        }
    }
    IEnumerator Bullet()
    {
        if (playerController.criticalEnemy == null)
        {
            normalEnemy = null;
            specialEnemy = null;
            hits = Physics.OverlapBox(transform.position, new Vector3(range / 2, 1, range / 2), Quaternion.identity, enemyLayer);
            foreach (Collider enemy in hits)
            {
                if (enemy.gameObject.CompareTag("Enemy") && normalEnemy == null)
                    normalEnemy = enemy.GetComponent<EnemyContoller>();
                else if (enemy.gameObject.CompareTag("SpecialEnemy"))
                {
                    specialEnemy = enemy.GetComponent<EnemyContoller>();
                    break;
                }
            }
            if (specialEnemy)
                chosenEnemy = specialEnemy;
            else
                chosenEnemy = normalEnemy;
        }
        else
            chosenEnemy = playerController.criticalEnemy;

        Quaternion lookRotation = transform.rotation;
        if (chosenEnemy)
        {
            Vector3 direction = (chosenEnemy.transform.position - transform.position);
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        float infrontOfWallDistance = 0.1f;

        if (Physics.Raycast(transform.position, lookRotation * Vector3.forward, out hitInfo, range*2, shootingLayer))
        {
            if (hitInfo.transform.tag == "Enemy" || hitInfo.transform.tag == "SpecialEnemy")
            {
                Instantiate(bloodEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                hitInfo.collider.gameObject.GetComponent<EnemyContoller>().TakeDamage(damage);
            }
            else
            {
                Instantiate(wallDecalEffect, hitInfo.point + hitInfo.normal * infrontOfWallDistance, Quaternion.LookRotation(hitInfo.normal));
            }
        }

        Quaternion initialRotation = transform.rotation;
        for (float t = 0f; t < 1f; t += 5f * Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, lookRotation, t);
            yield return null;
        }

    }
    public void AddClip()
    {
        currentClips = Mathf.Clamp(currentClips++, 0, maxClips);
    }

    // GUI
    void OnGUI()
    {
        if (!HUD_companion)
        {
            try
            {
                HUD_companion = GameObject.Find("HUD_companion").GetComponent<TextMesh>();
            }
            catch (System.Exception ex)
            {
                print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
            }
        }
        if (HUD_companion)
            HUD_companion.text = currentClips.ToString() + " - " + bulletsIHave.ToString();
    }
}
