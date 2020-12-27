using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyContoller : MonoBehaviour
{
    NavMeshAgent nm;
    public Transform target;
    public enum AIState { idle, chasing, attack, patrol };
    public AIState aiState = AIState.idle;
    public Animator animator;
    private float attackDistance = 0.5f;
    private Vector3[] patrolling;
    private int patrollingIdx = 0;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Parent");
        nm = GetComponent<NavMeshAgent>();
        // target = GameObject.FindGameObjectWithTag("Player").transform;
        // StartCoroutine(Think());
        patrolling = new Vector3[2];
        patrolling[0] = transform.position + new Vector3(2, 0, 0);
        patrolling[1] = transform.position + new Vector3(-2, 0, 0);
    }
    public virtual void getInitialHealth()
    {
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
        if (InRange(target, transform, attackDistance))
        {

            attack();
        }
        //  || sound() || raycast()
        else if (InRange(target, transform, 5f))
        {
            chase();
        }
        else
        {
            // int randomNumber = Random.Range(0, 2);

            // if (randomNumber == 0)
            //     idle();
            // else
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

}
