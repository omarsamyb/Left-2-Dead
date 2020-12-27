using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
    NavMeshAgent nm;
    public Transform target;
    public enum AIState { idle, chasing, attack };
    public AIState aiState = AIState.idle;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
        // target = GameObject.FindGameObjectWithTag("Player").transform;
        // StartCoroutine(Think());
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
    void Update()
    {
        if (InRange(target, transform, 0.5f)) // attack
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

            idle();
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
