using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class Hunter : EnemyContoller
{
    Vector3 jumpPosition;
    bool jumpingAttack;
    bool finishedJump;
    bool isKilling;
    Transform body;
    CapsuleCollider myCollider;
    protected override void Start()
    {
        base.Start();
        myCollider = GetComponent<CapsuleCollider>();
        body = transform.GetChild(0).GetChild(4);
        attackDistance = 2;
        reachDistance = attackDistance + 7;
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
            if (confusionTimer > 1.2f && !isKilling && !jumpingAttack && currentState != State.pipe && currentState != State.stunned)
                endConfusion(true);
            else if (confusionTimer > 1.2f && !isKilling)
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
            if (!isAlive(attackTarget))
                backToDefault();
            else if (canAttack && canAttackCheck(attackTarget))
            {
                if (navMeshAgent.remainingDistance < reachDistance)
                    attack();
                else //Can Attack but im far
                {
                    transform.LookAt(attackTarget);
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
                    transform.LookAt(attackTarget);
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
            if (canSee(reachDistance, attackAngle, attackTarget) && canAttack && canAttackCheck(attackTarget))
                attack();
            else if (finishedJump && jumpingAttack && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && canAttackCheck(attackTarget))
            {
                jumpingAttack = false;
                pinTarget();
            }
            else if(!canAttackCheck(attackTarget))
            {
                jumpingAttack = false;
                navMeshAgent.ResetPath();
            }
            else if (jumpingAttack)
                myCollider.center = new Vector3(myCollider.center.x, body.position.y, myCollider.center.z);
        }
        else if (currentState == State.pipe)
        {
            if (pipeTimer > 4.0f)
                pipeExploded(true);
            else
            {
                if (jumpingAttack)
                {
                    jumpingAttack = false;
                    navMeshAgent.speed = chaseSpeed;
                    StartCoroutine(resumeAttack());
                }
                else if (isKilling)
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
            if (jumpingAttack)
            {
                jumpingAttack = false;
                navMeshAgent.speed = chaseSpeed;
                StartCoroutine(resumeAttack());
            }
            else if (isKilling)
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
        transform.LookAt(attackTarget);
        animator.SetBool("isAttacking", true);
        jumpPosition = attackTarget.position;
        navMeshAgent.avoidancePriority = 0;
        StartCoroutine(jumpToTarget());
    }
    IEnumerator jumpToTarget()
    {
        finishedJump = false;
        yield return new WaitForSeconds(0.5f);
        if(currentState == State.attack)
        {
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            navMeshAgent.speed = 5;
            navMeshAgent.SetDestination(jumpPosition);
            jumpingAttack = true;
            ef.Attack(0);
        }
        yield return new WaitForSeconds(1f);
        finishedJump = true;
    }
    void pinTarget()
    {
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.ResetPath();
        if (Vector3.Distance(transform.position, attackTarget.position) <= attackDistance) //And player is not pinned
        {
            animator.SetTrigger("pin");
            myCollider.center = new Vector3(myCollider.center.x, 0.45f, myCollider.center.z);
            myCollider.height = 1.25f;
            myCollider.direction = 0;
            if (attackTarget.tag == "Player")
            {
                PlayerController cont = PlayerController.instance;
                cont.GetPinned(gameObject);
                StartCoroutine(attackAnyTarget(cont, null));
            }
            else if (attackTarget.tag.EndsWith("Enemy"))
            {
                EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
                cont.getPinned(true);
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
        if (playerCont == null)
        {
            if (enemyCont.health <= 0) return false;
            enemyCont.TakeDamage(damage);
            if (enemyCont.health <= 0) return false;
        }
        else
        {
            if (playerCont.health <= 0) return false;
            playerCont.TakeDamage(damage);
            if (playerCont.health <= 0) return false;
        }
        return true;
    }
    void unPinTarget(PlayerController playerCont, EnemyContoller enemyCont)
    {
        if(playerCont==null)
        {
            enemyCont.getUnpinned();
        }
        else
        {
            // playerCont.getUnpinned();
        }
    }
    IEnumerator attackAnyTarget(PlayerController playerCont, EnemyContoller enemyCont)
    {
        isKilling = true;
        ef.Attack(1);
        ef.source.loop = true;
        while (true)
        {
            if (currentState != State.attack || !doDamageOnTarget(playerCont, enemyCont, 10))
            {
                StartCoroutine(resumeAttack());
                isKilling = false;
                animator.SetBool("isAttacking", false);
                unPinTarget(playerCont, enemyCont);
                colliderToDefault();
                if (isConfused && confusionTimer > 1.2)
                {
                    if (currentState != State.stunned && currentState != State.pipe)
                        endConfusion(true);
                    else
                        endConfusion(false);
                }
                ef.source.loop = false;
                ef.source.Stop();

                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    void colliderToDefault()
    {
        navMeshAgent.avoidancePriority = 50;
        myCollider.center = new Vector3(myCollider.center.x, 1.072f, myCollider.center.z);
        myCollider.height = 2.160231f;
        myCollider.direction = 1;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }
}
