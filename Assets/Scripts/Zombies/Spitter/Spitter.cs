using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Spitter : EnemyContoller
{
    public static Vector3 attackingPosition;
    bool isAttacking = false;
    public override void attack()
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.ResetPath();
        myLookAt(attackTarget);
        attackingPosition = attackTarget.position;
        canAttack = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", true);
        currentState = State.attack;
        isAttacking = true;
        Invoke("stopAttacking", 2.8f);
        StartCoroutine(resumeAttack());
        StartCoroutine(SFX());
    }
    void stopAttacking(){isAttacking = false;}
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
            if (!isAlive(attackTarget))
                backToDefault();
            else if (canAttack && canAttackCheck(attackTarget))
            {
                if (navMeshAgent.remainingDistance < reachDistance)
                    attack();
                else //Can Attack but im far
                {
                    myLookAt(attackTarget);
                    if (Vector3.Distance(curGoToDestination, attackTarget.position) > distanceToUpdateDestination)//Don't update if unecessary
                    {
                        animator.SetBool("isChasing", true);
                        animator.SetBool("isIdle", false);
                        curGoToDestination = attackTarget.position;
                        navMeshAgent.SetDestination(curGoToDestination);
                    }
                }
            }
            else //Can't attack
            {
                if (Vector3.Distance(transform.position, attackTarget.position) > 3) //Im still far
                {
                    myLookAt(attackTarget);
                    if (Vector3.Distance(curGoToDestination, attackTarget.position) > distanceToUpdateDestination)//Don't update if unecessary
                    {
                        animator.SetBool("isChasing", true);
                        animator.SetBool("isIdle", false);
                        curGoToDestination = attackTarget.position;
                        navMeshAgent.SetDestination(curGoToDestination);
                    }
                }
                else //Im getting too close
                {
                    navMeshAgent.ResetPath();
                    animator.SetBool("isIdle", true);
                    animator.SetBool("isChasing", false);
                }

            }
        }
        else if (currentState == State.attack)
        {
            if(!isAttacking)
                chase(attackTarget);
            if (canAttack && isAlive(attackTarget))
                attack();
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
            else if (Vector3.Distance(hearedLocation, transform.position) < navMeshAgent.stoppingDistance)
                backToDefault();
        }
    }

    IEnumerator SFX()
    {
        yield return new WaitForSeconds(0.3f);
        ef.Attack(-1);
    }
}
