using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public class EnemyContoller : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public Transform playerTransform;
    public enum State { idle, chasing, attack, patrol, dead, stunned, pipe, hear };
    public State defaultState;
    [HideInInspector] public State currentState;
    public Animator animator;
    public float attackDistance = 1.0f, chaseDistance = 5.0f;
    public Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;
    private Transform attackTarget; // can be player of zombie (if confused)
    private float chaseSpeed = 2.0f;
    private float patrolSpeed = 0.5f;
    public Transform childTransform;
    private float chaseAngle = 130.0f;
    float attackCooldownTime = 1;
    bool canAttack = true;
    int damage = 5;
    private bool isConfused = false;
    private float stunTimer = 0, confusionTimer = 0, pipeTimer = 10;
    private Vector3 hearedLocation;
    Transform pipePosition;
    public void Confuse()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, chaseDistance, transform.forward, 0.0f);

        ArrayList enemies = new ArrayList();
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Enemy" && hit.transform.gameObject != this.gameObject && hit.transform.gameObject.GetComponent<EnemyContoller>().health > 0)
            {
                enemies.Add(hit.transform);
            }
        }

        if (enemies.Count == 0)
            return;
        confusionTimer = 0;
        isConfused = true;
        int random = Random.Range(0, enemies.Count);
        attackTarget = (Transform)enemies[random];
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
    }
    public void stun()
    {
        if (currentState == State.dead)
            return;
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("isPiped", false);
        animator.SetBool("isReachedPipe", false);
        animator.SetBool("isStunned", true);
        endConfusion(false);
        stunTimer = 0;
    }
    public void chase(Transform target)
    {

        if (currentState == State.dead)
            return;
        navMeshAgent.speed = chaseSpeed;
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        navMeshAgent.SetDestination(target.position);
    }
    public virtual void attack()
    {
        animator.SetBool("isChasing", false);
        if (currentState == State.dead)
            return;
        if (attackTarget.tag == "Player")
        {
            PlayerController cont = playerTransform.gameObject.GetComponent<PlayerController>();
            canAttack = false;
            currentState = State.attack;
            animator.SetBool("isAttacking", true);
            navMeshAgent.SetDestination(transform.position);
            StartCoroutine(applyDamage(cont));
        }
        else if (attackTarget.tag == "Enemy")
        {
            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();

            canAttack = false;
            currentState = State.attack;
            animator.SetBool("isAttacking", true);
            navMeshAgent.SetDestination(transform.position);
            cont.TakeDamage(damage);

        }
        StartCoroutine(resumeAttack());
    }
    void patrol()
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

        if (navMeshAgent.remainingDistance <= 0.5f)
        {
            patrollingIdx++;
            if (patrollingIdx >= patrolling.Length)
                patrollingIdx = 0;
        }
        navMeshAgent.SetDestination(patrolling[patrollingIdx]);
    }
    void backToDefault()
    {
        if (currentState != State.dead)
        {
            if (defaultState == State.patrol)
                patrol();
            if (defaultState == State.idle)
                idle();
        }
    }
    void idle()
    {
        currentState = State.idle;
        animator.SetBool("isIdle", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        navMeshAgent.SetDestination(transform.position);
    }
    void endConfusion(bool callBacktoDefault)
    {
        isConfused = false;
        attackTarget = playerTransform;
        if (callBacktoDefault)
            backToDefault();
    }
    void endStun(bool callBacktoDefault)
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
    void pipeExploded(bool callBacktoDefault)
    {
        animator.SetBool("isReachedPipe", false);
        animator.SetBool("isPiped", false);
        if (callBacktoDefault)
            backToDefault();
    }
    IEnumerator resumeAttack()
    {
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
        animator.SetBool("isAttacking", false);
    }
    IEnumerator applyDamage(PlayerController cont) //Delayed damage on player for effect
    {
        yield return new WaitForSeconds(0.5f);
        if (health > 0 && currentState == State.attack)
            cont.TakeDamage(damage);
        print(cont.health);
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
            if (confusionTimer > 1f)
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
            if (canSee(attackDistance, 30f, attackTarget) && canAttack)
            {
                attack();
            }
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
            if (!canSee(attackDistance, 30f, attackTarget))
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
                if (navMeshAgent.remainingDistance < 0.5f)
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

    private bool InRange(Transform transform1, Transform transform2, float range)
    {
        float distance = Vector3.Distance(transform1.position, transform2.position);
        if (distance < range)
            return true;
        return false;
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }
    public void Die()
    {
        animator.SetBool("isDying", true);
        navMeshAgent.SetDestination(transform.position);
        currentState = State.dead;
        Destroy(gameObject, 7.0f);
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        navMeshAgent.ResetPath();
    }
    bool isAlive(Transform target)
    {
        if (target.gameObject.tag == "Player")
        {
            PlayerController cont = target.gameObject.GetComponent<PlayerController>();
            if (cont.health <= 0)
                return false;
        }
        else if (target.gameObject.tag == "Enemy")
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
        float angle = Vector3.Angle(transform.forward, direction);
        Ray ray = new Ray(transform.position, direction);
        bool checkDistance = Physics.Raycast(ray, out RaycastHit hit, rangeDistance);
        if (checkDistance && hit.transform == target && Mathf.Abs(angle) < rangeAngle)
            return true;
        return false;
    }

    private float CalculatePathLength(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();

        if (navMeshAgent.enabled)
            navMeshAgent.CalculatePath(targetPosition, path);


        float pathLength = 0f;

        if (path.corners.Length == 0)
            pathLength = Vector3.Distance(transform.position, targetPosition);
        else
        {
            pathLength = Vector3.Distance(transform.position, path.corners[0]);
            pathLength += Vector3.Distance(path.corners[path.corners.Length - 1], targetPosition);
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            pathLength += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return pathLength;
    }
    public void canHearPlayer(float radius)
    {
        if (currentState == State.chasing || currentState == State.attack || currentState == State.dead || currentState == State.stunned) return;
        if (CalculatePathLength(playerTransform.position) < radius)
        {
            currentState = State.chasing;
        }
    }
    public void pipeGrenade(Transform grenadePosition, bool resetTimer = true)
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
    public void hearFire()
    {
        if (currentState == State.idle || currentState == State.patrol || currentState == State.hear)
        {
            hearedLocation = playerTransform.position;
            currentState = State.hear;
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(hearedLocation);
            animator.SetBool("isChasing", true);
            animator.SetBool("isIdle", false);
        }
    }
    void OnCollisionEnter(Collision other)
    {
        print(other.gameObject.name);
        // print(other.gameObject.tag);
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PlayerController>().health > 0)
            chase(other.transform);
    }
}
