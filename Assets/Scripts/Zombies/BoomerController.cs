//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;
//using UnityEditor;
//public class BoomerController : EnemyContoller
//{

//    public GameObject bomb;
//    public float chasesDistance = 3.0f;
//    public float boomerAttackDistance = 10.0f;

//    //                   0        1             2             3             4           5             6             7            8            9       10
//    string[] arr = { "isIdle", "isPatrol", "isChasing", "isAttacking", "isStunned", "isPiped", "isReachedPipe", "isDying", "chargerPin", "hunterPin", "isAttackingZombies" };

//    protected override void Start()
//    {
//        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
//        playerTransform = PlayerController.instance.player.transform;
//        attackTarget = playerTransform;
//        currentState = defaultState;
//        navMeshAgent = GetComponent<NavMeshAgent>();
//        animator.SetBool("isIdle", defaultState == State.idle);
//        animator.SetBool("isPatrol", defaultState == State.patrol);
//        reachDistance = attackDistance + 0.5f;
//        healthBar.SetMaxHealth(health);
//        ef = transform.GetComponent<EnemyEffects>();
//        pvo = PlayerController.instance.transform.GetComponent<PlayerVoiceOver>();
//    }
//    public override void stun()
//    {
//        if (currentState == State.dead)
//            return;
//        currentState = State.stunned;
//        navMeshAgent.ResetPath();
//        SetAnimationFlags(4);
//        endConfusion(false);
//        stunTimer = 0;
//    }
//    public override void chase(Transform target)
//    {
//        if (currentState == State.dead)
//            return;
//        navMeshAgent.speed = chaseSpeed;
//        currentState = State.chasing;
//        SetAnimationFlags(2);
//        curGoToDestination = target.position;
//        navMeshAgent.SetDestination(curGoToDestination);
//        if (!isChasing)
//        {
//            isChasing = true;
//            pvo.inFight = true;
//            pvo.requiredKills++;
//        }
//    }
//    public override void attack()
//    {
//        if (currentState == State.dead)
//            return;
//        transform.LookAt(attackTarget);
//        if (attackTarget.tag == "Player")
//        {
//            ef.Attack(-1);
//            PlayerController cont = PlayerController.instance;
//            canAttack = false;
//            currentState = State.attack;
//            SetAnimationFlags(3);
//            navMeshAgent.ResetPath();
//            // StartCoroutine(applyDamage(cont));
//            StartCoroutine(SetIdleAfterAttack());
//            StartCoroutine(resumeAttack());
//        }
//        else if (attackTarget.tag.EndsWith("Enemy"))
//        {
//            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
//            if (currentState == State.dead)
//                return;
//            transform.LookAt(attackTarget);
//            SetAnimationFlags(10);
//            canAttack = false;
//            currentState = State.attack;
//            navMeshAgent.ResetPath();
//            StartCoroutine(applyDamage(cont));
//            StartCoroutine(resumeAttack());

//        }
//    }
//    public override void patrol()
//    {
//        if (currentState == State.dead)
//            return;
//        navMeshAgent.speed = patrolSpeed;
//        SetAnimationFlags(1);
//        if (currentState != State.patrol)
//        {
//            currentState = State.patrol;
//            patrollingIdx = 0;
//        }

//        if (navMeshAgent.remainingDistance <= 0.5f)
//        {
//            patrollingIdx++;
//            if (patrollingIdx >= patrolling.Length)
//                patrollingIdx = 0;
//        }
//        navMeshAgent.SetDestination(patrolling[patrollingIdx]);
//    }
//    public override void idle()
//    {
//        currentState = State.idle;
//        SetAnimationFlags(0);
//        navMeshAgent.ResetPath();
//    }

//    public override void endStun(bool callBacktoDefault)
//    {
//        if (pipeTimer <= 4)
//        {
//            currentState = State.pipe;
//            pipeGrenade(pipePosition, false);
//        }
//        else if (callBacktoDefault)
//            backToDefault();
//    }
//    public override void pipeExploded(bool callBacktoDefault)
//    {
//        if (callBacktoDefault)
//            backToDefault();
//    }

//    void Update()
//    {
//        childTransform.position = transform.position;
//        if (pipeTimer < 4)
//            pipeTimer = pipeTimer + Time.deltaTime;
//        if (currentState == State.dead || isPinned)
//            return;

//        if (isConfused)
//        {
//            confusionTimer += Time.deltaTime;
//            if (confusionTimer > 1.2f)
//            {
//                endConfusion(true);
//            }
//        }
//        if (currentState == State.patrol)
//        {
//            if (canSee(chaseDistance, chaseAngle, attackTarget))
//            {
//                chase(attackTarget);
//            }
//            else
//            {
//                patrol();
//            }
//        }

//        else if (currentState == State.chasing)
//        {
//            if (navMeshAgent.remainingDistance < attackDistance && canAttack)
//            {
//                attack();
//            }
//            // calculate distance between zombie and player 
//            else if ((Vector3.Distance(transform.position, attackTarget.position) < (chasesDistance)))
//            {
//                navMeshAgent.ResetPath();
//                SetAnimationFlags(0);
//            }
//            else
//            {
//                // keep chasing
//                if (isAlive(attackTarget))
//                {
//                    transform.LookAt(attackTarget);
//                    if (Vector3.Distance(curGoToDestination, attackTarget.position) > distanceToUpdateDestination)//Don't update if unecessary
//                    {
//                        curGoToDestination = attackTarget.position;
//                        navMeshAgent.SetDestination(curGoToDestination);
//                    }
//                    SetAnimationFlags(2);
//                }
//                else
//                    backToDefault();
//            }
//        }
//        else if (currentState == State.attack && !animator.GetBool("isAttacking"))
//        {
//            if (!canSee(reachDistance, attackAngle, attackTarget))
//            {
//                chase(attackTarget);
//            }
//            else if (canAttack && isAlive(attackTarget))
//            {
//                attack();
//            }
//            else if (!canAttack && isAlive(attackTarget) && animator.GetBool("isIdle"))
//            {
//                chase(attackTarget);

//            }
//        }
//        else if (currentState == State.pipe)
//        {
//            if (pipeTimer > 4.0f)
//                pipeExploded(true);
//            else
//            {
//                navMeshAgent.SetDestination(pipePosition.position);
//                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f)
//                {
//                    SetAnimationFlags(6);
//                    navMeshAgent.ResetPath();
//                }
//            }
//        }
//        else if (currentState == State.idle)
//        {
//            if (canSee(chaseDistance, chaseAngle, attackTarget))
//                chase(attackTarget);
//        }
//        else if (currentState == State.stunned)
//        {
//            stunTimer = stunTimer + Time.deltaTime;
//            if (stunTimer > 3.0f)
//                endStun(true);
//        }
//        else if (currentState == State.hear)
//        {
//            if (canSee(chaseDistance, chaseAngle, attackTarget))
//                chase(attackTarget);
//            else if (Vector3.Distance(hearedLocation, transform.position) < 0.5f)
//                backToDefault();
//        }
//    }

//    public override void Die()
//    {
//        SetAnimationFlags(7);
//        currentState = State.dead;
//        Destroy(healthBarUI);
//        Destroy(gameObject, 7.0f);
//        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
//        navMeshAgent.ResetPath();
//    }
//    public override void pipeGrenade(Transform grenadePosition, bool resetTimer = true)
//    {
//        if (resetTimer)
//            pipeTimer = 0;
//        pipePosition = grenadePosition;
//        if (currentState == State.dead || currentState == State.stunned) return;
//        navMeshAgent.SetDestination(grenadePosition.position);
//        currentState = State.pipe;
//        navMeshAgent.speed = chaseSpeed;
//        SetAnimationFlags(5);
//        endConfusion(false); //If i am in pipe, ignore bile effect
//    }
//    public override void hearFire()
//    {
//        if (currentState == State.idle || currentState == State.patrol || currentState == State.hear)
//        {
//            hearedLocation = playerTransform.position;
//            currentState = State.hear;
//            navMeshAgent.speed = chaseSpeed;
//            navMeshAgent.SetDestination(hearedLocation);
//            SetAnimationFlags(2);
//        }
//    }
//    IEnumerator SetIdleAfterAttack()
//    {
//        yield return new WaitForSeconds(1.5f);
//        transform.LookAt(attackTarget);

//        //childGrenade.transform.parent = gameObject.transform;
//        if (currentState == State.attack && canAttackCheck(attackTarget))
//        {
//            GameObject childGrenade = Instantiate(bomb, new Vector3(transform.position.x, transform.position.y + 1.7f, transform.position.z), transform.rotation);
//            SetAnimationFlags(0);
//        }
//    }
//    void SetAnimationFlags(int g)
//    {
//        // kol haga false 
//        for (int i = 0; i < arr.Length; i++)
//        {
//            animator.SetBool(arr[i], i == g);
//        }
//    }
//    public override void Confuse()
//    {
//        base.Confuse();
//        canAttack = true;
//        attackDistance = 1f;
//    }
//    public override void endConfusion(bool callBacktoDefault)
//    {
//        base.endConfusion(callBacktoDefault);
//        attackDistance = boomerAttackDistance;
//    }
//}
