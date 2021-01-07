using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class Hunter : EnemyContoller
{
   Vector3 jumpPosition;
   bool jumpingAttack;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        attackDistance = 1;
        reachDistance = attackDistance + 7;
    }
   void Update()
    {
        childTransform.position = transform.position;
        if (pipeTimer < 4)
            pipeTimer = pipeTimer + Time.deltaTime;
        if (currentState == State.dead)
            return;
        if (isConfused)
        {
            confusionTimer += Time.deltaTime;
            if (confusionTimer > 1.2f)
                endConfusion(true);
        }
        if (currentState == State.patrol)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
            else
                patrol();
        }

        else if (currentState == State.chasing)
        {
            if (navMeshAgent.remainingDistance < reachDistance && canAttack)
                attack();
            else
            {
                // keep chasing
                if (isAlive(attackTarget))
                    navMeshAgent.SetDestination(attackTarget.position);
                else
                    backToDefault();
            }
        }
        else if (currentState == State.attack)
        {
            if (canSee(reachDistance, attackAngle, attackTarget) && canAttack && isAlive(attackTarget))
                attack();  
            else if(jumpingAttack && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                jumpingAttack = false;
                pinTarget();
            }
        }
        else if (currentState == State.pipe)
        {
            if (pipeTimer > 4.0f)
                pipeExploded(true);
            else
            {
                if(jumpingAttack)
                {
                    jumpingAttack = false;
                    StartCoroutine(resumeAttack());
                }
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
            if(jumpingAttack)
            {
                jumpingAttack = false;
                StartCoroutine(resumeAttack());
            }
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
   public override void attack()
   {
        navMeshAgent.ResetPath();
        animator.SetBool("isChasing", false);
        if (currentState == State.dead)
            return;
        canAttack = false;
        currentState = State.attack;
        FaceTarget(attackTarget.position);
        animator.SetBool("isAttacking", true);
        jumpPosition = attackTarget.position;
        StartCoroutine(jumpToTarget());
   }
   IEnumerator jumpToTarget()
   {
        yield return new WaitForSeconds(0.73f);
        navMeshAgent.speed = 5;
        navMeshAgent.SetDestination(jumpPosition);
        jumpingAttack = true;
   }
   void pinTarget()
    {
        navMeshAgent.ResetPath();
        if(Vector3.Distance(transform.position, attackTarget.position) <= attackDistance) //And player is not pinned
        {
            animator.SetTrigger("pin");
            if(attackTarget.tag=="Player")
            {
                PlayerController cont = playerTransform.gameObject.GetComponent<PlayerController>();
                //call pin at player
                StartCoroutine(attackAnyTarget(cont, null));
            }    
            else if(attackTarget.tag.EndsWith("Enemy"))
            {
                EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
                //call pin at zombie
                StartCoroutine(attackAnyTarget(null, cont));
            }
        }
        else
        {
            animator.SetTrigger("noPin");
            chase(attackTarget);
            StartCoroutine(resumeAttack());
        }
    }
    void doDamageOnTarget(PlayerController playerCont, EnemyContoller enemyCont, int damage)
    {
        if(playerCont==null)
            enemyCont.TakeDamage(damage);
        else
            playerCont.TakeDamage(damage);
    }
    IEnumerator attackAnyTarget(PlayerController playerCont, EnemyContoller enemyCont)
    {
        animator.SetBool("isAttacking", false);
        for(int i=0;i<7;i++)
        {
            doDamageOnTarget(playerCont, enemyCont, 10);
            yield return new WaitForSeconds(1f);
            if(currentState!=State.attack)
            {
                StartCoroutine(resumeAttack());
                yield break;
            }
        }
    }
}
