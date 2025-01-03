﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public enum State { idle, chasing, attack, patrol, dead, stunned, pipe, hear, coolDown };
public class EnemyContoller : MonoBehaviour
{
    protected NavMeshAgent navMeshAgent;
    protected Transform playerTransform;
    public State defaultState;
    [HideInInspector] public State currentState;
    public Animator animator;
    public float attackDistance = 1.0f, chaseDistance = 5.0f;
    public float reachDistance;
    public Vector3[] patrolling;
    protected int patrollingIdx = 0;
    public int health;
    [HideInInspector] public Transform attackTarget; // can be player of zombie (if confused)
    [HideInInspector] public float chaseSpeed = 2.0f;
    [HideInInspector] public float patrolSpeed = 0.2f;
    public Transform childTransform;
    protected float chaseAngle = 70.0f, attackAngle = 40.0f;
    public float attackCooldownTime = 1;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool canHitReaction = true;
    [HideInInspector] public float hitReactionDelay = 5.0f;
    public int damagePerSec = 5;
    protected bool isConfused = false;
    protected float stunTimer = 0, confusionTimer = 0, pipeTimer = 10;
    protected Vector3 hearedLocation;
    protected Transform pipePosition;
    protected Vector3 curGoToDestination;
    protected float distanceToUpdateDestination = 0.5f;
    public HealthBar healthBar;
    public GameObject healthBarUI;
    public GameObject bloodEffect;
    [HideInInspector] public bool isPinned;
    public Transform hitPoint;
    protected LayerMask enemyLayer;
    //Patroling     
    [HideInInspector]public Vector3 walkPoint;    
    bool walkPointSet;     
    [HideInInspector]public float walkPointRange;     
    [HideInInspector]public LayerMask whatIsGround;      
    public bool patrolRandom;


    protected PlayerVoiceOver pvo;
    protected bool isChasing;
    protected EnemyEffects ef;
    public virtual void Confuse()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, chaseDistance, enemyLayer);
        List<Transform> enemies = new List<Transform>();
        foreach (Collider hit in hits)
        {
            if (hit.transform.gameObject != this.gameObject && hit.transform.gameObject.GetComponent<EnemyContoller>().health > 0)
            {
                enemies.Add(hit.transform);
            }
        }

        if (enemies.Count == 0)
            return;
        confusionTimer = 0;
        int min = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (Vector3.Distance(((Transform)enemies[i]).position, transform.position) < Vector3.Distance(((Transform)enemies[min]).position, transform.position))
                min = i;

        }
        attackTarget = (Transform)enemies[min];
        if (!isConfused && currentState != State.attack)
            chase(attackTarget);
        isConfused = true;
    }

    protected virtual void Start()
    {
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        playerTransform = PlayerController.instance.player.transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        reachDistance = attackDistance + 0.5f;
        healthBar.SetMaxHealth(health);
        
        pvo = PlayerController.instance.transform.GetComponent<PlayerVoiceOver>();
        ef = transform.GetComponent<EnemyEffects>();

        walkPointRange = 5.0f; 
        whatIsGround = 1 << LayerMask.NameToLayer("Ground");
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }
    
    public void getPinned(bool isPerm)
    {
        isPinned = true;
        navMeshAgent.ResetPath();
        clearAnimator();
        if (isPerm)
        {
            animator.SetBool("hunterPin", true);
        }
        else
        {
            animator.SetBool("chargerPin", true);
        }
        //animator
    }
    public void getUnpinned()
    {
        isPinned = false;
        animator.SetBool("hunterPin", false);
        animator.SetBool("chargerPin", false);
        backToDefault();
    }
    public virtual void stun()
    {
        if (currentState == State.dead)
            return;
        currentState = State.stunned;
        navMeshAgent.ResetPath();
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("isPiped", false);
        animator.SetBool("isReachedPipe", false);
        animator.SetBool("isStunned", true);
        endConfusion(false);
        stunTimer = 0;
    }
    public virtual void chase(Transform target)
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = chaseSpeed;
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        curGoToDestination = target.position;
        navMeshAgent.SetDestination(curGoToDestination);
        if (!isChasing)
        {
            isChasing = true;
            pvo.inFight = true;
            pvo.requiredKills++;
        }
    }
    public virtual void attack()
    {
        animator.SetBool("isChasing", false);
        if (currentState == State.dead)
            return;
        myLookAt(attackTarget);
        canAttack = false;
        currentState = State.attack;
        animator.SetBool("isAttacking", true);
        navMeshAgent.ResetPath();
        if (attackTarget.tag == "Player")
        {
            PlayerController cont = PlayerController.instance;
            StartCoroutine(applyDamage(cont));
        }
        else if (attackTarget.tag.EndsWith("Enemy"))
        {
            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
            StartCoroutine(applyDamage(cont));
        }
        StartCoroutine(resumeAttack());
    }
    public virtual void patrol()
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = patrolSpeed;
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
            patrollingIdx = 0;
        }
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);

       /*6 if (navMeshAgent.remainingDistance <= 1f)
        {
            patrollingIdx++;
            if (patrollingIdx >= patrolling.Length)
                patrollingIdx = 0;
            navMeshAgent.SetDestination(patrolling[patrollingIdx]);
        }*/
        if (patrolRandom)
        {
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
                navMeshAgent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }
    }
    void clearAnimator()
    {
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStunned", false);
        animator.SetBool("isPiped", false);
        animator.SetBool("isReachedPipe", false);
        animator.SetBool("isIdle", false);
    }
    public void backToDefault()
    {
        clearAnimator();
        if (currentState != State.dead)
        {
            if (defaultState == State.patrol)
                patrol();
            if (defaultState == State.idle)
                idle();
        }
    }
    public virtual void idle()
    {
        currentState = State.idle;
        animator.SetBool("isIdle", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        navMeshAgent.ResetPath();
    }
    public virtual void endConfusion(bool callBacktoDefault)
    {
        isConfused = false;
        attackTarget = playerTransform;
        if (callBacktoDefault)
            backToDefault();
    }
    public virtual void endStun(bool callBacktoDefault)
    {
        animator.SetBool("isStunned", false);
        if (pipeTimer <= 4)
        {
            currentState = State.pipe;
            pipeGrenade(pipePosition, false);
        }
        else if (callBacktoDefault)
            backToDefault();
    }
    public virtual void pipeExploded(bool callBacktoDefault)
    {
        animator.SetBool("isReachedPipe", false);
        animator.SetBool("isPiped", false);
        if (callBacktoDefault)
            backToDefault();
    }
    public virtual IEnumerator resumeAttack()
    {
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
    }
    public virtual IEnumerator resumeHitReaction()
    {
        yield return new WaitForSeconds(hitReactionDelay);
        canHitReaction = true;
    }
    public virtual IEnumerator applyDamage(PlayerController cont) //Delayed damage on player for effect
    {
        yield return new WaitForSeconds(0.5f);
        ef.Attack(-1);
        if (cont.health > 0 && currentState == State.attack)
            cont.TakeDamage(damagePerSec);
    }
    public virtual IEnumerator applyDamage(EnemyContoller cont) //Delayed damage on player for effect
    {
        yield return new WaitForSeconds(0.5f);
        ef.Attack(-1);
        if (cont.health > 0 && currentState == State.attack)
        {
            cont.TakeDamage(damagePerSec, attackTarget.position + new Vector3(0, 1.5f, 0));
            if(cont.health <= 0f)
                pvo.fightKills++;
            
        }
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
            if (navMeshAgent.remainingDistance < attackDistance && canAttack)
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

    private bool InRange(Transform transform1, Transform transform2, float range)
    {
        float distance = Vector3.Distance(transform1.position, transform2.position);
        if (distance < range)
            return true;
        return false;
    }
    public void TakeDamage(int damage)
    {
        ef.Damaged();
        health -= damage;
        healthBar.SetHealth(health);
        if (health <= 0)
            Die();
    }
    public void TakeDamage(int damage, Vector3 pos)
    {
        ef.Damaged();
        health -= damage;
        healthBar.SetHealth(health);
        Instantiate(bloodEffect, pos, Quaternion.identity);
        //audioSource.PlayOneShot(hurtSFX);
        if (health <= 0)
            Die();
        if (canHitReaction)
        {
            canHitReaction = false;
            animator.SetTrigger("gotHit");
            StartCoroutine(resumeHitReaction());
        }
        hearFire();
    }
    public virtual void Die()
    {
        ef.Dead();
        animator.SetBool("isDying", true);
        currentState = State.dead;
        Destroy(healthBarUI);
        Destroy(gameObject, 7.0f);
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        navMeshAgent.ResetPath();
    }
    public bool isAlive(Transform target)
    {
        if (target.gameObject.tag == "Player")
        {
            PlayerController cont = target.gameObject.GetComponent<PlayerController>();
            if (cont.health <= 0)
                return false;
        }
        else if (target.gameObject.tag.EndsWith("Enemy"))
        {
            EnemyContoller cont = target.gameObject.GetComponent<EnemyContoller>();
            if (cont.health <= 0)
                return false;
        }
        return true;
    }
    public bool canSee(float rangeDistance, float rangeAngle, Transform target)
    {
        if (!isAlive(target))
            return false;
        Vector3 direction = (target.position - transform.position).normalized;
        direction *= rangeDistance;
        float angle = Vector3.Angle(transform.forward, direction);
        Ray ray = new Ray(transform.position + new Vector3(0, 1.0f, 0), direction);
        bool checkDistance = Physics.Raycast(ray, out RaycastHit hit, rangeDistance);
        if (checkDistance && Mathf.Abs(angle) < rangeAngle && hit.transform.position == target.position)
            return true;
        return false;
    }


    public void canHearPlayer(float radius)
    {
        if (currentState == State.chasing || currentState == State.attack || currentState == State.dead || currentState == State.stunned)
            return;
        if (Vector3.Distance(transform.position, playerTransform.position) < radius)
            currentState = State.chasing;
    }
    public virtual void pipeGrenade(Transform grenadePosition, bool resetTimer = true)
    {
        if (resetTimer)
            pipeTimer = 0;
        pipePosition = grenadePosition;
        if (currentState == State.dead || currentState == State.stunned) return;
        navMeshAgent.SetDestination(grenadePosition.position);
        currentState = State.pipe;
        navMeshAgent.speed = chaseSpeed;
        animator.SetBool("isPiped", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdle", false);
        endConfusion(false); //If i am in pipe, ignore bile effect
    }
    public string getState()
    {
        return currentState.ToString();
    }
    public virtual void hearFire()
    {
        if ((currentState == State.idle || currentState == State.patrol || currentState == State.hear) && !isPinned)
        {
            hearedLocation = playerTransform.position;
            currentState = State.hear;
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(hearedLocation);
            animator.SetBool("isChasing", true);
            animator.SetBool("isIdle", false);
        }
    }
    protected bool canAttackCheck(Transform target)
    {
        if (target.tag == "Player")
        {
            PlayerController cont = PlayerController.instance;
            return !cont.isPinned && cont.health > 0;
        }
        else
        {
            EnemyContoller cont = target.GetComponent<EnemyContoller>();
            return !cont.isPinned && cont.health > 0;
        }
    }
    void OnCollisionEnter(Collision other)
    {
        if (!isPinned && other.gameObject.tag == "Player" && PlayerController.instance.health > 0 && ((currentState == State.idle) || (currentState == State.chasing)))
        {
            myLookAt(other.transform);
            chase(other.transform);
        }
    }
    public void myLookAt(Transform placeToLook)
    {
        float damping = 1.5f;
        Vector3 lookPos = placeToLook.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
    }
}
