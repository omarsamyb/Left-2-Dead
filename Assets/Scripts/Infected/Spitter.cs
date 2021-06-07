using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Spitter : InfectedController
{
    // Spit Effect
    [SerializeField] private GameObject spitGO;
    [SerializeField] private ParticleSystem spitPS;
    [SerializeField] private GameObject spitPuddleGO;
    private AudioSource puddleSource;
    private float startSpeed;
    private float modifier;
    private Vector3 spitHitPos;
    private float spitPuddleRadius;
    private LayerMask playerLayer;

    protected override void Start()
    {
        base.Start();
        health = 100;
        dph = 20;
        patrolSpeed = 1f;
        chaseSpeed = 3f;
        stoppingDistance = 6f;
        attackRange = 96f;
        attackDelayTime = 5f;
        attackTime = attackDelayTime / 2f;
        attackDelay = new WaitForSeconds(attackDelayTime);
        attackType = AttackType.ranged;
        modifier = 0.5f;
        spitPuddleGO.transform.SetParent(DynamicObjects.instance.transform, false);
        spitPuddleGO.SetActive(false);
        spitPuddleRadius = 2f;
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        puddleSource = spitPuddleGO.GetComponent<AudioSource>();
        healthBar.SetMaxHealth(health);
        isForwardDeathAnimation = true;
        height = 1.5f;
    }
    protected override void Update()
    {
        base.Update();
        if (state == InfectedState.chase || state == InfectedState.attack)
        {
            attackTime += Time.deltaTime;
            if (state == InfectedState.attack && target && !inAttackSequence)
                FaceTarget();
            if (inAttackSequence && target)
            {
                Quaternion spitRotation = Quaternion.LookRotation((target.position - transform.position).normalized);
                spitGO.transform.rotation = Quaternion.Euler(spitRotation.eulerAngles.x, spitRotation.eulerAngles.y, 0f);
            }
        }
        if (state != InfectedState.attack)
        {
            spitGO.SetActive(false);
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
                attackTime = 0f;
                startSpeed = Mathf.Sqrt(distanceToTarget) / modifier;
                var main = spitPS.main;
                main.startSpeed = startSpeed;

                if (NavMesh.SamplePosition(target.position, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                {
                    spitPuddleGO.transform.position = navMeshHit.position + spitPuddleGO.transform.up * 0.1f;
                    spitHitPos = navMeshHit.position;
                }
                else
                    Debug.LogWarning("Could not generate spit puddle - target not near a ground");

                animator.SetTrigger("isAttacking");
                StartCoroutine(PuddleDamage());
                yield return new WaitForEndOfFrame();
                yield return new WaitWhile(() => animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
                inAttackSequence = false;
            }
            inAttackSequence = false;
            attackTime = 0f;
            yield return null;
            animator.SetTrigger("inCooldown");
            yield return attackDelay;
        }
    }
    private IEnumerator PuddleDamage()
    {
        bool savedTargetType = targetType;
        yield return new WaitForSeconds(1.2f);
        spitPuddleGO.SetActive(false);
        spitPuddleGO.SetActive(true);
        for (int i = 0; i < 5; i++)
        {
            if (savedTargetType)
            {
                Collider[] hits = Physics.OverlapBox(spitHitPos, new Vector3(spitPuddleRadius, 0.4f, spitPuddleRadius), Quaternion.identity, playerLayer);
                if (hits.Length > 0)
                    PlayerController.instance.TakeDamage(20);
            }
            else
            {
                Collider[] hits = Physics.OverlapBox(spitHitPos, new Vector3(spitPuddleRadius, 0.4f, spitPuddleRadius), Quaternion.identity, infectedLayer);
                foreach (Collider collider in hits)
                {
                    InfectedController infected = collider.GetComponent<InfectedController>();
                    if(targetType != savedTargetType)
                        infected.TakeDamage(20, -1);
                    else
                    {
                        attackedInfected = infected;
                        ApplyDamage(1);
                    }

                }
            }
            yield return new WaitForSeconds(1f);
        }
        puddleSource.Stop();
    }
    public void ActivateSpit()
    {
        spitGO.SetActive(true);
    }
    public void DeactivateSpit()
    {
        spitGO.SetActive(false);
    }
    public void SoundFX()
    {
        infectedEffects.Attack(-1);
    }
}
