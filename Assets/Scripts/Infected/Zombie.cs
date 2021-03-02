using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : InfectedController
{
    private NavMeshObstacle obstacle;
    protected override void Start()
    {
        base.Start();
        health = 50;
        dps = 5;
        stoppingDistance = 1.5f;
        patrolSpeed = 0.5f;
        chaseSpeed = 3f;
        attackRange = 4f;
        attackType = AttackType.melee;
        healthBar.SetMaxHealth(health);
        obstacle = GetComponent<NavMeshObstacle>();
    }
    protected override void Update()
    {
        base.Update();
        if(state == InfectedState.attack)
            attackTime += Time.deltaTime;
        if(agent.enabled && ReachedDestination())
        {
            agent.enabled = false;
            obstacle.enabled = true;
        }
        if(!agent.enabled && distanceToTarget > Mathf.Pow(agent.stoppingDistance, 2))
        {
            obstacle.enabled = false;
            agent.enabled = true;
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
        attackTime = 0f;
        animator.SetBool("attackingMirror", Random.Range(0, 2) == 0 ? true : false);
        animator.SetTrigger("isAttacking");
        while (true)
        {
            FaceTarget();
            if (!ConditionsCheck(8))
            {
                animator.SetTrigger("inCooldown");
                yield return new WaitUntil(() => ConditionsCheck(8));
                animator.SetTrigger("isAttacking");
            }
            yield return null;
        }
    }
    public void AttackVariation()
    {
        animator.SetFloat("attackingMultiplier", Random.Range(1f, 1.4f));
    }
    public void Variation()
    {
        animator.SetFloat("multiplier", Random.Range(1f, 1.3f));
    }
    public void SoundFX()
    {
        infectedEffects.Attack(-1);
    }
}
