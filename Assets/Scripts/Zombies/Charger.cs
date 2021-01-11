using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class Charger : EnemyContoller
{
    Vector3 chargePosition;
    bool runningAttack;
    CapsuleCollider myCollider;
    void Start()
    {
        playerTransform = PlayerController.instance.player.transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        attackDistance = 2;
        reachDistance = attackDistance + 6;
        runningAttack = false;
        healthBar.SetMaxHealth(health);
        myCollider = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();
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
                if(currentState == State.attack)
                    endConfusion(false);
                else
                    endConfusion(true);
                    
            }
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
            if (canSee(reachDistance, attackAngle, attackTarget) && canAttack &&  canAttackCheck(attackTarget))
                attack();
            else if (runningAttack && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && canAttackCheck(attackTarget))
            {
                runningAttack = false;
                pinTarget();
            }
        }
        else if (currentState == State.pipe)
        {
            if (pipeTimer > 4.0f)
                pipeExploded(true);
            else
            {
                if (runningAttack)
                {
                    runningAttack = false;
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
            if (runningAttack)
            {
                runningAttack = false;
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
        chargePosition = attackTarget.position;
        StartCoroutine(charge());
    }
    IEnumerator charge()
    {
        yield return new WaitForSeconds(3.2f);
        if (currentState != State.attack || isPinned || !canAttackCheck(attackTarget))
        {
            animator.SetBool("isAttacking", false);
            StartCoroutine(resumeAttack());
            yield break;
        }
        navMeshAgent.speed = 5;
        navMeshAgent.SetDestination(chargePosition);
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        runningAttack = true;
    }

    void pinTarget()
    {
        navMeshAgent.ResetPath();
        if (Vector3.Distance(transform.position, attackTarget.position) <= attackDistance) //And player is not pinned
        {
            animator.SetTrigger("pin");
            myCollider.height = 1.6f;
            myCollider.center = new Vector3(myCollider.center.x, 0.5f,myCollider.center.z);
            if (attackTarget.tag == "Player")
            {
                PlayerController cont = PlayerController.instance;
                PlayerController.instance.GetPartiallyPinned(transform.position, transform.position - transform.forward - transform.up * 1.2f, 2f, transform.position - transform.up);
                StartCoroutine(attackAnyTarget(cont, null));
            }
            else if (attackTarget.tag.EndsWith("Enemy"))
            {
                EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
                cont.getPinned(false);
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
        if (playerCont == null)
            enemyCont.TakeDamage(damage);
        else
            playerCont.TakeDamage(damage);
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
        animator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(2.33f);
        if (currentState != State.attack || isPinned)
        {
            StartCoroutine(resumeAttack());
            unPinTarget(playerCont, enemyCont);
            colliderToDefault();
            yield break;
        }
        doDamageOnTarget(playerCont, enemyCont, 15);
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(0.35f);
            if (currentState != State.attack || isPinned)
            {
                StartCoroutine(resumeAttack());
                unPinTarget(playerCont, enemyCont);
                colliderToDefault();
                yield break;
            }
            doDamageOnTarget(playerCont, enemyCont, 10);
        }
        colliderToDefault();
        chase(attackTarget);
        unPinTarget(playerCont, enemyCont);
        StartCoroutine(resumeAttack());
    }
    void colliderToDefault()
    {
        myCollider.center = new Vector3(myCollider.center.x, 1, myCollider.center.z);
        myCollider.height = 2.2f;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }
}
