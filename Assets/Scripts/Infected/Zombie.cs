using System.Collections;
using UnityEngine;

public class Zombie : InfectedController
{
    protected override void Start()
    {
        base.Start();
        health = 50;
        dps = 5;
        stoppingDistance = 1.5f;
        patrolSpeed = 0.5f;
        chaseSpeed = 3f;
        attackRange = 4f;
    }
    protected override void Update()
    {
        base.Update();
        if(state == InfectedState.attack)
            attackTime += Time.deltaTime;
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
            yield return null;
        }
    }
    public void AttackVariation()
    {
        animator.SetFloat("attackingMultiplier", Random.Range(1f, 1.4f));
    }
}
