using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Hunter : InfectedController
{
    private float pinRange;
    private float chargeSpeed;
    private float maxJumpDistance;
    [SerializeField] private CapsuleCollider collisionCollider;

    protected override void Start()
    {
        base.Start();
        health = 250;
        dph = 10;
        patrolSpeed = 0.5f;
        chaseSpeed = 3f;
        suspiciousWalkSpeed = 2f;
        chargeSpeed = 7f;
        stoppingDistance = 8f;
        attackRange = 140f;
        attackDelayTime = 5f;
        attackTime = attackDelayTime;
        attackDelay = new WaitForSeconds(attackDelayTime);
        attackType = AttackType.pin;
        pinRange = 3.5f;
        maxJumpDistance = 36f;
        healthBar.SetMaxHealth(health);
        height = 1.5f;
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
            if (PlayerController.instance.isPinned && PlayerController.instance.criticalEnemy && ReferenceEquals(PlayerController.instance.criticalEnemy.transform, transform))
            {
                PlayerController.instance.isPinned = false;
                PlayerController.instance.criticalEnemy = null;
            }
            if(attackedInfected && attackedInfected.isPinned)
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
                agent.updateRotation = true;
                agent.stoppingDistance = 5f;
                agent.speed = chargeSpeed;
                agent.SetDestination(target.position);
                animator.SetTrigger("isAttacking");
                yield return new WaitUntil(() => ReachedDestination() || distanceToTarget <= Mathf.Pow(agent.stoppingDistance, 2f));
                if (distanceToTarget <= maxJumpDistance)
                {
                    agent.ResetPath();
                    Vector3 targetPos = target.position;
                    agent.updateRotation = false;
                    animator.SetTrigger("jump");
                    infectedEffects.Attack(0);
                    FaceTarget();
                    yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"));
                    collisionCollider.enabled = false;
                    Vector3 currentPos = transform.position;
                    float clipStart = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                    {
                        transform.position = Vector3.Lerp(currentPos, targetPos, (animator.GetCurrentAnimatorStateInfo(0).normalizedTime - clipStart) / (1f - clipStart));
                        yield return null;
                    }
                    if (distanceToTarget <= pinRange)
                    {
                        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                        if (targetType)
                        {
                            if (!PlayerController.instance.isPartiallyPinned && !PlayerController.instance.isPinned)
                            {
                                PlayerController.instance.GetPinned(gameObject);
                                animator.SetTrigger("pin");
                            }
                        }
                        else
                        {
                            if (!attackedInfected.isPartiallyPinned && !attackedInfected.isPinned)
                            {
                                attackedInfected.GetPinned(transform.position);
                                animator.SetTrigger("pin");
                            }
                        }
                        while (true)
                        {
                            if (!targetType && bileTimer <= 0f)
                                break;
                            ApplyDamage(1);
                            yield return new WaitForSeconds(1f);
                        }
                    }
                    collisionCollider.enabled = true;
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