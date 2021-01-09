﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class Hunter : EnemyContoller
{
   Vector3 jumpPosition;
   bool jumpingAttack;
   Vector3 curGoToDestination;
   float distanceToUpdateDestination;
   bool isKilling;
   Transform body;
   CapsuleCollider myCollider;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        attackDistance = 1;
        reachDistance = attackDistance + 7;
        distanceToUpdateDestination = 0.5f;
        body = transform.GetChild(0).GetChild(4);
        myCollider = GetComponent<CapsuleCollider>();
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
            if (confusionTimer > 1.2f && !isKilling && !jumpingAttack && currentState!=State.pipe && currentState!=State.stunned)
                endConfusion(true);
            else if(confusionTimer > 1.2f && !isKilling)
                endConfusion(false);
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
            if(!isAlive(attackTarget))
                backToDefault();
            else if (canAttack)
            {
                if(navMeshAgent.remainingDistance < reachDistance)
                    attack();
                else //Can Attack but im far
                {
                    if(Vector3.Distance(curGoToDestination, attackTarget.position)>distanceToUpdateDestination)//Don't update if unecessary
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
                if(Vector3.Distance(transform.position, attackTarget.position)>3) //Im still far
                {
                    if(Vector3.Distance(curGoToDestination, attackTarget.position)>distanceToUpdateDestination)//Don't update if unecessary
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
            if (canSee(reachDistance, attackAngle, attackTarget) && canAttack && isAlive(attackTarget))
                attack();  
            else if(jumpingAttack && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                jumpingAttack = false;
                pinTarget();
            }
            else if(jumpingAttack)
                myCollider.center = new Vector3(myCollider.center.x,body.position.y,myCollider.center.z);
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
                    navMeshAgent.speed = chaseSpeed;
                    StartCoroutine(resumeAttack());
                }
                else if(isKilling)
                {
                    isKilling = false;
                    colliderToDefault();
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
                navMeshAgent.speed = chaseSpeed;
                StartCoroutine(resumeAttack());
            }
            else if(isKilling)
            {
                isKilling = false;
                colliderToDefault();
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
        navMeshAgent.avoidancePriority = 0;
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
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.ResetPath();
        if(Vector3.Distance(transform.position, attackTarget.position) <= attackDistance) //And player is not pinned
        {
            animator.SetTrigger("pin");
            myCollider.center = new Vector3(myCollider.center.x,0.45f,myCollider.center.z);
            myCollider.height = 1.25f;
            myCollider.direction = 0;
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
            colliderToDefault();
            chase(attackTarget);
            StartCoroutine(resumeAttack());
        }
    }
    bool doDamageOnTarget(PlayerController playerCont, EnemyContoller enemyCont, int damage)
    {
        if(playerCont==null)
        {
            if(enemyCont.health<=0)return false;
            enemyCont.TakeDamage(damage);
            if(enemyCont.health<=0) return false;
        }
        else
        {
            if(playerCont.health<=0)return false;
            playerCont.TakeDamage(damage);
            if(playerCont.health<=0)return false;
        }
        return true;
    }
    IEnumerator attackAnyTarget(PlayerController playerCont, EnemyContoller enemyCont)
    {
        isKilling = true;
        while(true)
        {
            if(currentState!=State.attack || !doDamageOnTarget(playerCont, enemyCont, 10))
            {
                StartCoroutine(resumeAttack());
                isKilling = false;
                animator.SetBool("isAttacking", false);
                colliderToDefault();
                if(isConfused && confusionTimer>1.2)
                {
                    if(currentState!=State.stunned && currentState!=State.pipe)
                        endConfusion(true);
                    else
                        endConfusion(false);
                }
                    
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    void colliderToDefault()
    {
        navMeshAgent.avoidancePriority = 50;
        myCollider.center = new Vector3(myCollider.center.x,1.072f,myCollider.center.z);
        myCollider.height = 2.160231f;
        myCollider.direction = 1;
    }
}
