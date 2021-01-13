using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Spitter : EnemyContoller
{
    public override void attack()
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.ResetPath();
        transform.LookAt(attackTarget);
        canAttack = false;
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", true);
        currentState = State.attack;
        
        StartCoroutine(resumeAttack());
        StartCoroutine(SFX());
    }

    IEnumerator SFX()
    {
        yield return new WaitForSeconds(0.3f);
        ef.Attack(-1);
    }
}
