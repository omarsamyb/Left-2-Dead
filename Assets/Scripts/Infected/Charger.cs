using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Charger : InfectedController
{
    private float pinRange;
    private float chargeSpeed;
    private string punchStateName;

    protected override void Start()
    {
        base.Start();
        health = 600;
        dph = 15; // 5 hits
        stoppingDistance = 5f;
        patrolSpeed = 1f;
        chaseSpeed = 3f;
        suspiciousWalkSpeed = 2f;
        chargeSpeed = 7f;
        attackRange = 120f;
        attackDelayTime = 5f;
        attackTime = attackDelayTime;
        attackDelay = new WaitForSeconds(attackDelayTime);
        attackType = AttackType.pin;
        pinRange = 3.5f;
        punchStateName = "Punch";
        healthBar.SetMaxHealth(health);
    }
    protected override void Update()
    {
        base.Update();
        if (state == InfectedState.chase || state == InfectedState.attack)
        {
            attackTime += Time.deltaTime;
            if (state == InfectedState.attack && target && !agent.updateRotation)
                FaceTarget();
        }
        if (state != InfectedState.attack)
        {
            inAttackSequence = false;
            if (PlayerController.instance.isPartiallyPinned && PlayerController.instance.criticalEnemy && ReferenceEquals(PlayerController.instance.criticalEnemy.transform, transform))
            {
                PlayerController.instance.isPartiallyPinned = false;
                PlayerController.instance.criticalEnemy = null;
            }
            if (attackedInfected && attackedInfected.isPartiallyPinned)
                attackedInfected.Unpin();
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
                agent.updateRotation = false;
                animator.SetTrigger("isAttacking");
                infectedEffects.Attack(0);
                yield return new WaitForEndOfFrame();
                yield return new WaitWhile(() => animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
                agent.stoppingDistance = 2f;
                agent.speed = chargeSpeed;
                agent.updateRotation = true;
                agent.SetDestination(target.position);
                animator.SetTrigger("charge");
                yield return new WaitUntil(() => ReachedDestination() || distanceToTarget <= pinRange);
                if (distanceToTarget <= pinRange)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    FaceTarget();
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    if (targetType)
                    {
                        if (!PlayerController.instance.isPartiallyPinned && !PlayerController.instance.isPinned)
                        {
                            PlayerController.instance.GetPartiallyPinned(transform.position + transform.up * 0.6f, transform.position - transform.forward * 0.9f - transform.up * 1.2f, 4.5f, transform.position + transform.up, gameObject);
                            yield return new WaitForEndOfFrame();
                            animator.SetTrigger("pin");
                            yield return new WaitForEndOfFrame();
                            yield return new WaitWhile(() => !animator.GetCurrentAnimatorStateInfo(0).IsName(punchStateName) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
                            PlayerController.instance.isPartiallyPinned = false;
                        }
                    }
                    else
                    {
                        if (!attackedInfected.isPartiallyPinned && !attackedInfected.isPinned)
                        {
                            Vector3 pinPos = transform.position - transform.forward;
                            animator.SetTrigger("pin");
                            yield return new WaitForSeconds(0.8f);
                            attackedInfected.GetPartiallyPinned(transform.position, pinPos);
                            yield return new WaitWhile(() => !animator.GetCurrentAnimatorStateInfo(0).IsName(punchStateName) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
                            attackedInfected.Unpin();
                        }
                    }
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                }
            }
            inAttackSequence = false;
            agent.updateRotation = false;
            attackTime = 0f;
            yield return null;
            animator.SetTrigger("inCooldown");
            yield return attackDelay;
        }
    }
    public void SoundFX()
    {
        infectedEffects.Attack(1);
    }
}