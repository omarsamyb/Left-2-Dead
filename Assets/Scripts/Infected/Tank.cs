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
        attackDelayTime = 1f;
        attackDelay = new WaitForSeconds(attackDelayTime);
    }
    protected override void Update()
    {
        base.Update();
        if (state == InfectedState.chase || state == InfectedState.attack)
        {
            attackTime += Time.deltaTime;
            if (state == InfectedState.attack && target)
                FaceTarget();
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
            animator.SetTrigger("isIdle");
            yield return new WaitUntil(() => attackTime >= attackDelayTime);
        }
        while (true)
        {
            animator.SetTrigger("isAttacking");
            animator.SetInteger("attackIndex", Random.Range(0, 5));
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
            animator.SetTrigger("isIdle");
            yield return attackDelay;
        }
    }
}
