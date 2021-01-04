using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyContoller : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    public Transform playerTransform;
    public enum State { idle, chasing, attack, patrol, dead, stunned, pipe, hear };
    private State currentState;
    public Animator animator;
    public float attackDistance = 1.0f, chaseDistance = 5.0f;
    private Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;
    private Transform attackTarget; // can be player of zombie (if confused)
    private float stunTimer = 0, confusionTimer = 0; private float pipeTimer = 0;
    private bool isConfused;
    private float chaseSpeed = 2.0f;
    private float patrolSpeed = 0.3f;
    public Transform childTransform;
    private float chaseAngle = 130.0f;
    private Vector3 hearedLocation;
    public void Confuse()
    {

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, chaseDistance, transform.forward, 0.0f);

        ArrayList enemies = new ArrayList();
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Enemy" && hit.transform.gameObject != this.gameObject)
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
        currentState = State.patrol;
        navMeshAgent = GetComponent<NavMeshAgent>();
        patrolling = new Vector3[2];
        patrolling[0] = transform.position + 2 * transform.forward;
        patrolling[1] = transform.position - 2 * transform.forward;
    }
    public void stun()
    {
        stunTimer = 0;
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStunned", true);
    }
    public void chase(Transform target)
    {
        navMeshAgent.speed = chaseSpeed;
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        navMeshAgent.SetDestination(target.position);
    }
    void attack()
    {

        currentState = State.attack;
        animator.SetBool("isAttacking", true);
        navMeshAgent.SetDestination(transform.position);
    }
    // void idle()
    // {
    //     currentState = State.idle;
    //     animator.SetBool("isAttacking", false);
    //     animator.SetBool("isChasing", false);
    //     navMeshAgent.SetDestination(transform.position);
    // }
    void patrol()
    {
        navMeshAgent.speed = patrolSpeed;
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
            patrolling[0] = transform.position + 2 * transform.forward;
            patrolling[1] = transform.position - 2 * transform.forward;
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
    void Update()
    {
        childTransform.position = transform.position;
        if (currentState == State.dead)
            return;
        if (currentState == State.stunned)
        {
            stunTimer = stunTimer + Time.deltaTime;
            if (stunTimer > 3.0f)
            {
                animator.SetBool("isStunned", false);
                patrol();
            }
            else
                return;
        }
        if (isConfused)
        {
            confusionTimer += Time.deltaTime;
            if (confusionTimer > 5.0f) {
                patrol();
                attackTarget = playerTransform;
                isConfused = false;
                confusionTimer = 0;
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
        else if (currentState == State.chasing) {
            if (canSee(attackDistance, 30f, attackTarget))
            {
                attack();
            }
            else
            {
                // keep chasing
                navMeshAgent.SetDestination(attackTarget.position);
            }
        }
        else if (currentState == State.attack) {
            if (!canSee(attackDistance, 30f, attackTarget))
            {
                chase(attackTarget);
            }
        }
        else if (currentState == State.pipe)
        {
            pipeTimer = pipeTimer + Time.deltaTime;
            if (pipeTimer > 4.0f)
            {
                animator.SetBool("isReachedPipe", false);
                animator.SetBool("isPiped", false);
                patrol();
            }
            else if (navMeshAgent.remainingDistance < 1f)
            {
                animator.SetBool("isReachedPipe", true);
            }

        }
        else if (currentState == State.hear)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
            {
                chase(attackTarget);
            }
            else if (Vector3.Distance(hearedLocation, transform.position) < 2f)
            {
                navMeshAgent.speed = patrolSpeed;
                animator.SetBool("isChasing", false);
                patrol();
            }
        }
    }

    private bool InRange(Transform transform1, Transform transform2, float range)
    {
        float distance = Vector3.Distance(transform1.position, transform2.position);
        if (distance < range)
        {
            return true;
        }
        return false;
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {

            Die();
        }
    }
    public void Die()
    {
        animator.SetBool("isDying", true);
        navMeshAgent.SetDestination(transform.position);
        currentState = State.dead;
        Destroy(gameObject,7.0f);
        this.gameObject.GetComponent<CapsuleCollider>().enabled=false;
    }
    public bool canSeePlayer(float rangeDistance, float rangeAngle){
        return canSee(rangeDistance,rangeAngle,playerTransform);
    }
    public bool canSee(float rangeDistance, float rangeAngle,Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        Ray ray = new Ray(transform.position, direction);
        bool checkDistance = Physics.Raycast(ray, out RaycastHit hit, rangeDistance);
        if (checkDistance && hit.transform==target && Mathf.Abs(angle) < rangeAngle)
        {
            return true;
        }
        return false;


    }

    private float CalculatePathLength(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();

        if(navMeshAgent.enabled)
        navMeshAgent.CalculatePath(targetPosition, path);


        float pathLength = 0f;

        if (path.corners.Length == 0)
        {
            pathLength = Vector3.Distance(transform.position, targetPosition);

        }
        else
        {
            pathLength = Vector3.Distance(transform.position, path.corners[0]);
            pathLength += Vector3.Distance(path.corners[path.corners.Length-1], targetPosition);
        }
        for (int i = 0; i < path.corners.Length-1; i++)
        {
            pathLength+= Vector3.Distance(path.corners[i], path.corners[i+1]);
        }

        return pathLength;
    }
    public void canHearPlayer(float radius)
    {
        if (currentState == State.chasing || currentState == State.attack|| currentState == State.dead|| currentState == State.stunned) return;
        if (CalculatePathLength(playerTransform.position) < radius)
        {
            currentState = State.chasing;
        }
    }
    public void pipeGrenade( Transform grenadePosition)
    {
        // float grenadeRadius, as parameter 

        if (currentState == State.dead || currentState == State.stunned) return;

        //if (CalculatePathLength(grenadePosition) < grenadeRadius)
        //{
        //    navMeshAgent.SetDestination(grenadePosition);
        //    currentState = State.pipe;
        //    animator.SetBool("isPiped", true);
        //    animator.SetBool("isAttacking", false);
        //    animator.SetBool("isChasing", false);
        //    pipeTimer = 0f;
        //    // what I am missing here the animator controller to add isPipe 
        //}
            navMeshAgent.SetDestination(grenadePosition.position);
            currentState = State.pipe;
            navMeshAgent.speed= chaseSpeed;
            animator.SetBool("isPiped", true);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isChasing", false);
            pipeTimer = 0f;
    }
    public string getState()
    {
        return currentState.ToString();
    }
    public void hearFire()
    {
        if (currentState == State.idle || currentState == State.patrol || currentState==State.hear)
        {
            hearedLocation = playerTransform.position;
            currentState = State.hear;
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(hearedLocation);
            animator.SetBool("isChasing", true);
        }
    }
}
