using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Boomer : InfectedController
{
    // Vomit Effect
    [SerializeField] private GameObject vomitGO;
    [SerializeField] private ParticleSystem[] vomitPS;
    [SerializeField] private GameObject vomitPuddleGO;
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private AudioClip spawnSFX;
    private AudioSource puddleSource;
    public GameObject zombiePrefab;
    private float startSpeed;
    private float modifier;
    [HideInInspector] public bool collided;
    private bool inSpawnRoutine;
    private WaitForSeconds activationDelay;
    private Vector3 vomitHitPos;
    private int zombieSpawnRate;
    private int spawnDurationRef;
    private int spawnDuration;
    private bool canSpawn;

    protected override void Start()
    {
        base.Start();
        health = 50;
        stoppingDistance = 6f;
        patrolSpeed = 0.5f;
        chaseSpeed = 3f;
        attackRange = 96f;
        attackDelayTime = 10f;
        attackTime = attackDelayTime;
        attackDelay = new WaitForSeconds(attackDelayTime);
        activationDelay = new WaitForSeconds(1f);
        attackType = AttackType.ranged;
        modifier = 0.5f;
        vomitPuddleGO.transform.SetParent(DynamicObjects.instance.transform, false);
        vomitPuddleGO.SetActive(false);
        spawnDurationRef = 4;
        spawnDuration = spawnDurationRef;
        zombieSpawnRate = 4;
        puddleSource = vomitPuddleGO.GetComponent<AudioSource>();
        healthBar.SetMaxHealth(health);
    }
    protected override void Update()
    {
        base.Update();
        if (state == InfectedState.chase || state == InfectedState.attack)
        {
            attackTime += Time.deltaTime;
            if (state == InfectedState.attack && target && !inAttackSequence)
                FaceTarget();
            if(inAttackSequence && target)
                vomitGO.transform.rotation = Quaternion.Euler(Quaternion.LookRotation((target.position - transform.position).normalized).eulerAngles.x, vomitGO.transform.parent.rotation.eulerAngles.y, 0f);
        }
        if (state != InfectedState.attack)
        {
            vomitGO.SetActive(false);
            inAttackSequence = false;
        }
    }

    protected override bool Attack()
    {
        if (base.Attack())
            attackRoutine = StartCoroutine(AttackRoutine());
        return true;
    }
    private IEnumerator AttackRoutine()
    {
        print(gameObject.name + " Starting Attack Routine");
        inAttackRoutine = true;
        agent.updateRotation = false;
        if (attackTime < attackDelayTime)
        {
            animator.SetTrigger("inCooldown");
            yield return new WaitUntil(() => attackTime >= attackDelayTime);
        }
        while (true)
        {
            if (ConditionsCheck(8))
            {
                inAttackSequence = true;
                collided = false;
                inSpawnRoutine = false;
                canSpawn = false;
                attackTime = 0f;
                startSpeed = Mathf.Sqrt(distanceToTarget) / modifier;
                foreach (ParticleSystem ps in vomitPS)
                {
                    var main = ps.main;
                    main.startSpeed = startSpeed;
                }

                if (NavMesh.SamplePosition(target.position, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                {
                    vomitPuddleGO.transform.position = navMeshHit.position + vomitPuddleGO.transform.up * 0.1f;
                    vomitHitPos = navMeshHit.position;
                    canSpawn = true;
                }
                else
                    Debug.LogWarning("Could not generate vomit puddle - target not near a ground");

                animator.SetTrigger("isAttacking");
                yield return new WaitForEndOfFrame();
                while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                {
                    if (collided && !inSpawnRoutine && canSpawn)
                    {
                        canSpawn = false;
                        StartCoroutine(SpawnZombies(bileTimer > 0f ? true : false));
                    }
                    yield return null;
                }
                inAttackSequence = false;
            }
            inAttackSequence = false;
            attackTime = 0f;
            yield return null;
            animator.SetTrigger("inCooldown");
            yield return attackDelay;
        }
    }
    private IEnumerator SpawnZombies(bool biled)
    {
        inSpawnRoutine = true;
        if (biled)
            spawnDuration = 1;
        else
        {
            spawnDuration = spawnDurationRef;
            PlayerController.instance.BileVisionEffect();
        }
        for (int i = 0; i < spawnDuration; i++)
        {
            Zombie[] spawned = new Zombie[zombieSpawnRate];
            for (int j = 0; j < zombieSpawnRate; j++)
            {
                while (true)
                {
                    randomPoint = vomitHitPos + Random.insideUnitSphere * 2f;
                    if (NavMesh.SamplePosition(randomPoint, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                    {
                        spawned[j] = Instantiate(zombiePrefab, navMeshHit.position, Quaternion.Euler(0f, Random.Range(0f, 90f), 0f)).GetComponent<Zombie>();
                        spawned[j].state = InfectedState.empty;
                        spawned[j].criticalEvent = true;
                        spawned[j].transform.GetComponent<Animator>().SetTrigger("isResurrected");
                        break;
                    }
                    else
                        continue;
                }
            }
            Destroy(Instantiate(spawnEffect, vomitHitPos, Quaternion.identity, DynamicObjects.instance.transform), 1.2f);
            StartCoroutine(ActivateZombies(spawned, biled));
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator ActivateZombies(Zombie[] zombies, bool biled)
    {
        yield return activationDelay;
        foreach (Zombie zombie in zombies)
        {
            if (zombie.state != InfectedState.dead)
            {
                zombie.criticalEvent = false;
                if (biled)
                    zombie.Bile(false, true);
                else
                    zombie.ChasePlayer();
            }
        }
    }
    public void ActivateVomit()
    {
        infectedEffects.Attack(-1);
        vomitGO.SetActive(true);
        vomitPuddleGO.SetActive(false);
        vomitPuddleGO.SetActive(true);
    }
    public void DeactivateVomit()
    {
        vomitGO.SetActive(false);
    }
}
