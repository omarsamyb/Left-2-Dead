using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionController : MonoBehaviour
{
    public static CompanionController instance;
    public string weaponName;
    public int maxClips = 3;
    public GameObject muzzelSpawn;

    private GameObject[] muzzelFlash;
    private AudioClip shootSFX;
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
    private float runningSpeed;
    private float walkingSpeed;
    private bool inCoroutine;
    private bool isJumping;
    [HideInInspector] public int killCounter;
    [HideInInspector] public bool canApplyAbility;
    private CompanionVoiceOver cvo;
    private PlayerVoiceOver pvo;
    private float weaponNoiseCoolDownRef = 0.5f;
    private float weaponNoiseCoolDown;
    private float noiseRange = 10f;


    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        GunScript weapon = ((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + weaponName + ".prefab", typeof(GameObject))).GetComponent<GunScript>();
        muzzelFlash = weapon.muzzelFlash;
        shootSFX = weapon.shootSFX;
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
        runningSpeed = agent.speed;
        walkingSpeed = agent.speed / 2f;
        cvo = GetComponent<CompanionVoiceOver>();
        pvo = PlayerController.instance.transform.GetComponent<PlayerVoiceOver>();
        weaponNoiseCoolDown = weaponNoiseCoolDownRef;

        agent.SetDestination(player.position);
    }

    void Update()
    {
        if (PlayerController.instance.health > 0)
        {
            Shooting();
            Movement();

            if (killCounter >= 10)
            {
                killCounter = 0;
                AddClip();
            }

            if (weaponNoiseCoolDown > 0)
                weaponNoiseCoolDown -= Time.deltaTime;
        }
    }
    private void Movement()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (agent.isOnOffMeshLink && !isJumping)
        {
            isJumping = true;
            animator.SetTrigger("isJumping");
        }
        else if (!agent.isOnOffMeshLink && isJumping)
            isJumping = false;

        if (distance > agent.stoppingDistance)
        {
            if (!inCoroutine)
                StartCoroutine(GoToPlayer());
        }

        if (distance < agent.stoppingDistance + agent.radius * 2)
            canApplyAbility = true;
        else
            canApplyAbility = false;

        if (!agent.pathPending && agent.enabled)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude <= 1f)
                {
                    animator.SetInteger("speed", 0);
                    agent.ResetPath();
                    agent.enabled = false;
                    obstacle.enabled = true;
                }
            }
        }
    }
    IEnumerator GoToPlayer()
    {
        inCoroutine = true;
        if (!agent.enabled)
        {
            obstacle.enabled = false;
            yield return null;
            agent.enabled = true;
        }

        agent.SetDestination(player.position);
        if (agent.remainingDistance < agent.stoppingDistance * 1.8f)
        {
            animator.SetInteger("speed", 1);
            agent.speed = walkingSpeed;
        }
        else
        {
            animator.SetInteger("speed", 2);
            agent.speed = runningSpeed;
        }
        yield return new WaitForSeconds(0.2f);
        inCoroutine = false;
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
            else
                animator.SetBool("isShooting", false);
        }
        waitTillNextFire -= roundsPerSecond * Time.deltaTime;
    }
    private void ShootWeapon()
    {
        if (waitTillNextFire <= 0 && bulletsIHave > 0)
        {
            cvo.StartCoroutine(cvo.Order());
            if (!agent.enabled)
            {
                if(style == GunStyles.automatic)
                    animator.SetBool("isShooting", true);
                else
                    animator.SetTrigger("isShooting");
            }
            int randomNumberForMuzzelFlash = Random.Range(0, 5);
            StartCoroutine(Bullet());
            Instantiate(muzzelFlash[randomNumberForMuzzelFlash], muzzelSpawn.transform.position, muzzelSpawn.transform.rotation * Quaternion.Euler(0, 0, 90), muzzelSpawn.transform);
            fireSource.Play();
            waitTillNextFire = 1;
            if (!GameManager.instance.inRageMode)
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
        bool isCriticalEnemy = false;
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
        {
            chosenEnemy = playerController.criticalEnemy;
            isCriticalEnemy = true;
        }

        Quaternion lookRotation = transform.rotation;
        Vector3 direction = Vector3.zero;
        if (chosenEnemy)
        {
            direction = (chosenEnemy.hitPoint.position - transform.position);
            lookRotation = Quaternion.LookRotation(direction);
        }

        float infrontOfWallDistance = 0.1f;

        if (Physics.Raycast(transform.position, lookRotation * Vector3.forward, out hitInfo, range * 2, shootingLayer))
        {
            if (hitInfo.transform.CompareTag("Enemy") || hitInfo.transform.CompareTag("SpecialEnemy"))
            {
                EnemyContoller enemy = hitInfo.collider.gameObject.GetComponent<EnemyContoller>();
                enemy.TakeDamage(damage, hitInfo.point);
                if (enemy.health <= 0)
                {
                    cvo.StartCoroutine(cvo.Kill());
                    pvo.fightKills++;
                    killCounter++;
                    if(isCriticalEnemy && hitInfo.transform.CompareTag("SpecialEnemy"))
                    {
                        PlayerController.instance.criticalEnemy = null;
                        PlayerController.instance.isPinned = false;
                    }
                }
            }
            else
            {
                Instantiate(wallDecalEffect, hitInfo.point + hitInfo.normal * infrontOfWallDistance, Quaternion.LookRotation(hitInfo.normal));
            }
        }

        Quaternion initialRotation = transform.rotation;
        if(chosenEnemy)
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        for (float t = 0f; t < 1f; t += 5f * Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, lookRotation, t);
            yield return null;
        }

        if (weaponNoiseCoolDown <= 0)
        {
            weaponNoiseCoolDown = weaponNoiseCoolDownRef;
            hits = Physics.OverlapBox(transform.position, new Vector3(noiseRange, 1f, noiseRange), Quaternion.identity, enemyLayer);
            foreach (Collider collider in hits)
            {
                collider.GetComponent<EnemyContoller>().hearFire();
            }
        }

    }
    public void AddClip()
    {
        currentClips = Mathf.Clamp(++currentClips, 0, maxClips);
    }
}
