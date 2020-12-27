using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyContoller : MonoBehaviour
{
    NavMeshAgent nm;
    public Transform target;
    public enum AIState { idle, chasing, attack, patrol, dead, stunned };
    public AIState aiState = AIState.idle;
    public Animator animator;
    private float attackDistance = 0.5f;
    private Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;

    private float timer = 0;

    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        patrolling = new Vector3[2];
        patrolling[0] = transform.position + new Vector3(2, 0, 0);
        patrolling[1] = transform.position + new Vector3(-2, 0, 0);
    }
    public void stun()
    {
        aiState = AIState.stunned;
        nm.SetDestination(transform.position);
        animator.SetBool("isStunned", true);
    }
    void chase()
    {
        aiState = AIState.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        nm.SetDestination(target.position);
    }
    void attack()
    {
        aiState = AIState.attack;
        animator.SetBool("isAttacking", true);
        // animator.SetBool("isChasing", false);
        nm.SetDestination(transform.position);
    }
    void idle()
    {
        aiState = AIState.idle;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        nm.SetDestination(transform.position);
    }
    void patrol()
    {
        aiState = AIState.patrol;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", true);
        nm.SetDestination(patrolling[patrollingIdx]);
        if (nm.remainingDistance <= 0.5f)
        {
            patrollingIdx++;
            if (patrollingIdx >= patrolling.Length)
                patrollingIdx = 0;
            nm.SetDestination(patrolling[patrollingIdx]);
        }
    }
    void Update()
    {
        if (aiState != AIState.patrol)
        {
            patrolling[0] = transform.position + new Vector3(2, 0, 0);
            patrolling[1] = transform.position + new Vector3(-2, 0, 0);
        }
        if (aiState == AIState.stunned)
        {
            timer = timer + Time.deltaTime;
            if (timer > 3.0f)
            {
                animator.SetBool("isStunned", false);
                animator.SetBool("isChasing", false);
                animator.SetBool("isAttacking", false);
                nm.SetDestination(transform.position);
                aiState = AIState.idle;
                timer = 0;
            }

            return;

        }
        // after take damage is actually called, remove second condition and nm.setdestination
        if (aiState == AIState.dead || animator.GetBool("isDying"))
        {
            nm.SetDestination(transform.position);
            return;
        }
        if (canSeePlayer(attackDistance, 90f))
        {
            attack();
        }
        //  || sound() 
        else if (canSeePlayer(5f, 90f))
        {
            chase();
        }
        else
        {
            patrol();
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
        nm.SetDestination(transform.position);
        aiState = AIState.dead;
    }

    public bool canSeePlayer(float rangeDistance, float rangeAngle)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        Ray ray = new Ray(transform.position, direction);
        bool checkDistance = Physics.Raycast(ray, out RaycastHit hit, rangeDistance);
        if (checkDistance && hit.collider.tag == "Player" && Mathf.Abs(angle) < rangeAngle)
        {
            return true;
        }
        return false;


    }

}
