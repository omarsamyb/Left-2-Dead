using System.Collections;
using UnityEngine;

public class Tank : InfectedController
{
    protected override void Start()
    {
        base.Start();
        health = 1000;
        dph = 30;
        stoppingDistance = 2f;
        patrolSpeed = 1.5f;
        chaseSpeed = 3f;
        attackRange = 6f;
        attackDelayTime = 2f;
        attackDelay = new WaitForSeconds(attackDelayTime);
        attackType = AttackType.melee;
        healthBar.SetMaxHealth(health);
        height = 2.2f;
    }
    protected override void Update()
    {
        base.Update();
        if (state == InfectedState.chase || state == InfectedState.attack)
        {
            attackTime += Time.deltaTime;
            if (state == InfectedState.attack && target && !inAttackSequence)
                FaceTarget();
        }
        if (state != InfectedState.attack)
        {
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
        if(attackTime < attackDelayTime)
        {
            animator.SetTrigger("inCooldown");
            yield return new WaitUntil(() => attackTime >= attackDelayTime);
        }
        while (true)
        {
            if (ConditionsCheck(8))
            {
                inAttackSequence = true;
                animator.SetTrigger("isAttacking");
                if(Random.Range(0, 3) == 0)
                    animator.SetInteger("attackIndex", 2);
                else
                    animator.SetInteger("attackIndex", Random.Range(0, 5));

                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                inAttackSequence = false;
                yield return null;
            }
            inAttackSequence = false;
            attackTime = 0f;
            yield return null;
            animator.SetTrigger("inCooldown");
            yield return attackDelay;
        }
    }
    public void SoundFX(int index)
    {
        infectedEffects.Attack(index);
    }
}
