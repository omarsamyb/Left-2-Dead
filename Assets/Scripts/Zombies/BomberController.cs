using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public class BomberController : EnemyContoller
{

    public GameObject bomb;

    //                   0        1           2            3            4           5         6                7
    string[] arr = { "isIdle", "isPatrol", "isChasing", "isAttacking", "isStunned", "isPiped", "isReachedPipe", "isDying" };

    public override void FaceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        animator.SetBool("isPatrol", defaultState == State.patrol);
        reachDistance = attackDistance + 0.5f;
        healthBar.SetMaxHealth(health);
        audioSource = GetComponent<AudioSource>();
    }
    public override void stun()
    {
        if (currentState == State.dead)
            return;
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        SetAnimationFlags(4);
        endConfusion(false);
        stunTimer = 0;
    }
    public override void chase(Transform target)
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = chaseSpeed;
        currentState = State.chasing;
        SetAnimationFlags(2);
        navMeshAgent.SetDestination(target.position);
    }
    public override void attack()
    {
        if (currentState == State.dead)
            return;
        FaceTarget(attackTarget.position);
        if (attackTarget.tag == "Player")
        {
            PlayerController cont = playerTransform.gameObject.GetComponent<PlayerController>();
            canAttack = false;
            currentState = State.attack;
            SetAnimationFlags(3);
            navMeshAgent.SetDestination(transform.position);
            StartCoroutine(applyDamage(cont));
        }
        else if (attackTarget.tag == "Enemy")
        {
            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();

            canAttack = false;
            currentState = State.attack;
            SetAnimationFlags(3);
            navMeshAgent.SetDestination(transform.position);
            cont.TakeDamage(damagePerSec);
        }
        StartCoroutine(SetIdleAfterAttack());
        StartCoroutine(resumeAttack());
    }
    public override void patrol()
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = patrolSpeed;
        SetAnimationFlags(1);
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
            patrollingIdx = 0;
        }

        if (navMeshAgent.remainingDistance <= 0.5f)
        {
            patrollingIdx++;
            if (patrollingIdx >= patrolling.Length)
                patrollingIdx = 0;
        }
        navMeshAgent.SetDestination(patrolling[patrollingIdx]);
    }
    public override void idle()
    {
        currentState = State.idle;
        SetAnimationFlags(0);
        navMeshAgent.SetDestination(transform.position);
    }

    public override void endStun(bool callBacktoDefault)
    {
        if (pipeTimer <= 4)
        {
            currentState = State.pipe;
            pipeGrenade(pipePosition, false);
        }
        else if (callBacktoDefault)
            backToDefault();
    }
    public override void pipeExploded(bool callBacktoDefault)
    {
        if (callBacktoDefault)
            backToDefault();
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
            if (navMeshAgent.remainingDistance < attackDistance && canAttack)
            {
                attack();
            }
            // calculate distance between zombie and player 
            else if ((Vector3.Distance(transform.position, attackTarget.position) < attackDistance))
            {
                navMeshAgent.ResetPath();
                SetAnimationFlags(0);
            }
            else
            {
                // keep chasing
                if (isAlive(attackTarget))
                {
                    navMeshAgent.SetDestination(attackTarget.position);
                    SetAnimationFlags(2);
                }
                else
                    backToDefault();
            }
        }
        else if (currentState == State.attack)
        {
            if (!canSee(reachDistance, attackAngle, attackTarget))
            {
                chase(attackTarget);
            }
            else if (canAttack && isAlive(attackTarget))
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
                    SetAnimationFlags(6);
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

    public override void Die()
    {
        SetAnimationFlags(7);
        navMeshAgent.SetDestination(transform.position);
        currentState = State.dead;
        Destroy(healthBarUI);
        Destroy(gameObject, 7.0f);
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        navMeshAgent.ResetPath();
    }
    public override void pipeGrenade(Transform grenadePosition, bool resetTimer = true)
    {
        if (resetTimer)
            pipeTimer = 0;
        pipePosition = grenadePosition;
        if (currentState == State.dead || currentState == State.stunned) return;
        navMeshAgent.SetDestination(grenadePosition.position);
        currentState = State.pipe;
        navMeshAgent.speed = chaseSpeed;
        SetAnimationFlags(5);
        endConfusion(false); //If i am in pipe, ignore bile effect
    }
    public override void hearFire()
    {
        if (currentState == State.idle || currentState == State.patrol || currentState == State.hear)
        {
            hearedLocation = playerTransform.position;
            currentState = State.hear;
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(hearedLocation);
            SetAnimationFlags(2);
        }
    }
    IEnumerator SetIdleAfterAttack()
    {
        yield return new WaitForSeconds(1.5f);
        FaceTarget(attackTarget.position);
        GameObject childGrenade = Instantiate(bomb, new Vector3(transform.position.x, transform.position.y + 1.7f, transform.position.z), transform.rotation);
        childGrenade.transform.parent = gameObject.transform;
        if (currentState == State.attack)
            SetAnimationFlags(0);
    }
    void SetAnimationFlags(int g)
    {
        // kol haga false 
        for (int i = 0; i < arr.Length; i++)
        {
            animator.SetBool(arr[i], i == g);
        }
    }
}
