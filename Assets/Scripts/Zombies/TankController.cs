﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public class TankController : EnemyContoller
{
    private bool inAttackAnimation=false;
    public override IEnumerator applyDamage(PlayerController cont)
    {
        float frames=50/2.0f;
        ef.Attack(0);
        yield return new WaitForSeconds(frames/30.0f);
        
        if (cont.health > 0 && currentState == State.attack && Vector3.Distance(transform.position,attackTarget.position)<attackDistance+0.5f && isFacingEachOther())
            cont.TakeDamage(damagePerSec);
    }
    public override IEnumerator applyDamage(EnemyContoller cont)
    {
        float frames=50/2.0f;
        yield return new WaitForSeconds(frames/30.0f);
        ef.Attack(0);
        if (cont.health > 0 && currentState == State.attack && Vector3.Distance(transform.position,attackTarget.position)<attackDistance+0.5f && isFacingEachOther())
            cont.TakeDamage(damagePerSec);
    }
    private bool isFacingEachOther(){
        float dot=Vector3.Dot(transform.forward, (attackTarget.position - transform.position).normalized);
        return dot>=0.2f;
    }
    public override IEnumerator resumeAttack()
    {
        animator.speed = 2;
        float animationTime=1.8f;
        inAttackAnimation=true;
        yield return new WaitForSeconds(animationTime);
        animator.speed = 1;
        inAttackAnimation=false;
        canAttack=true;
    }
    void Update()
    {
        childTransform.position = transform.position;
        if (pipeTimer < 4)
            pipeTimer = pipeTimer + Time.deltaTime;
        if (currentState == State.dead || isPinned)
            return;

        if (isConfused)
        {
            confusionTimer += Time.deltaTime;
            if (confusionTimer > 1.2f)
            {
                endConfusion(true);
            }
        }
        if (currentState == State.patrol)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
            {
                chase(attackTarget);
            }
            else
            {
                patrol();
            }
        }

        else if (currentState == State.chasing)
        {
            if (navMeshAgent.remainingDistance < attackDistance && canAttack && canAttackCheck(attackTarget))
            {
                attack();
            }
            else
            {
                // keep chasing
                if (isAlive(attackTarget))
                {
                    if (Vector3.Distance(curGoToDestination, attackTarget.position) > distanceToUpdateDestination)//Don't update if unecessary
                    {
                        curGoToDestination = attackTarget.position;
                        navMeshAgent.SetDestination(curGoToDestination);
                    }
                }
                else
                    backToDefault();
            }
        }
        else if (currentState == State.attack)
        {
            if (!inAttackAnimation && !canSee(attackDistance, attackAngle, attackTarget))
            {
                chase(attackTarget);
            }
            else if (canAttack && canAttackCheck(attackTarget))
            {
                attack();
            }
        }
        else if (currentState == State.pipe)
        {
            if (pipeTimer > 4.0f)
                pipeExploded(true);
            else
            {
                navMeshAgent.SetDestination(pipePosition.position);
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f)
                {
                    animator.SetBool("isReachedPipe", true);
                    navMeshAgent.ResetPath();
                }
            }
        }
        else if (currentState == State.idle)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
        }
        else if (currentState == State.stunned)
        {
            stunTimer = stunTimer + Time.deltaTime;
            if (stunTimer > 3.0f)
                endStun(true);
        }
        else if (currentState == State.hear)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
            else if (Vector3.Distance(hearedLocation, transform.position) < 0.5f)
                backToDefault();
        }
    }
}
