using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyContoller : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    public Transform playerTransform;
    public enum State { idle, chasing, attack, patrol, dead, stunned};
    private State currentState;
    public Animator animator;
    public float attackDistance = 1.0f,chaseDistance = 5.0f;
    private Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;
    private Transform attackTarget; // can be player of zombie (if confused)
    private float timer = 0, confusionTimer = 0;
    private bool isConfused;
    public void Confuse()
    {
       
        RaycastHit [] hits=Physics.SphereCastAll(transform.position,chaseDistance,transform.forward,0.0f);
       
        ArrayList enemies = new ArrayList(); 
        foreach (RaycastHit hit in hits)
        {
            if(hit.transform.gameObject.tag=="Enemy" && hit.transform.gameObject!=this.gameObject)
            {
                enemies.Add(hit.transform);
            }
        }
        
        if(enemies.Count == 0)
            return;
        confusionTimer = 0;
        isConfused = true;
        int random = Random.Range(0,enemies.Count);
        attackTarget = (Transform)enemies[random];

        
    }
    void Start()
    {
        attackTarget = playerTransform;
        currentState = State.patrol;
        navMeshAgent = GetComponent<NavMeshAgent>();
        patrolling = new Vector3[2];
        patrolling[0] = transform.position + new Vector3(2, 0, 0);
        patrolling[1] = transform.position + new Vector3(-2, 0, 0);
        Invoke("Confuse",4);
    }
    public void stun()
    {
        timer = 0;
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("isChasing",false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStunned", true);
    }
    void chase(Transform target)
    {
        navMeshAgent.speed= 2.0f;
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        navMeshAgent.SetDestination(target.position);
    }
    void attack()
    {
    
        currentState = State.attack;
        animator.SetBool("isAttacking", true);
        animator.SetBool("isChasing", false);
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
        navMeshAgent.speed=0.3f;
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
            patrolling[0] = transform.position + new Vector3(2, 0, 0);
            patrolling[1] = transform.position + new Vector3(-2, 0, 0);
            patrollingIdx=0;
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
        if(currentState == State.dead)
            return;
        if (currentState == State.stunned)
        {
            timer = timer + Time.deltaTime;
            if (timer > 3.0f)
            {
                animator.SetBool("isStunned", false);
                patrol();
            }
            else
                return;
        }
        if(isConfused)
        {
            confusionTimer += Time.deltaTime;
            if(confusionTimer > 5.0f){
                patrol();
                attackTarget = playerTransform;
                isConfused = false;
                confusionTimer = 0;
            }
        }
        if(currentState==State.patrol)
        {
            if (canSee(chaseDistance, 90f, attackTarget))
            {
                chase(attackTarget);
            }
            else
            {
                patrol();
            }
        }
        else if(currentState==State.chasing){
            if (canSee(attackDistance, 90f,attackTarget))
            {
                attack();
            }
            else
            {
                // keep chasing
               navMeshAgent.SetDestination(attackTarget.position);
            }
        }
        else if(currentState==State.attack){
            if (!canSee(attackDistance, 90f, attackTarget))
            {
                chase(attackTarget);
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

}
