using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
    NavMeshAgent nm;
    public Transform target;
    public enum AIState { idle,chasing,attack};
    public AIState aiState = AIState.idle;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        nm = GetComponent<NavMeshAgent>();
       // target = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(Think());
    }
    IEnumerator Think()
    {
        while (true)
        {
            switch (aiState)
            {
                case AIState.idle:
                    if(InRange(target,transform,5f))
                    {
                        aiState = AIState.chasing;
                        animator.SetBool("isChasing", true);
                    }
                    nm.SetDestination(transform.position);
                    break;
                case AIState.chasing:
                    if (!InRange(target, transform, 5f))
                    {
                        aiState = AIState.idle;
                        animator.SetBool("isChasing", false);
                    }
                    if(InRange(target, transform, 0.5f))
                    {
                        aiState = AIState.attack;
                        animator.SetBool("isAttacking", true);
                    }
                    nm.SetDestination(target.position); 
                    break;
                case AIState.attack:
                    nm.SetDestination(transform.position);
                    if (!InRange(target, transform, 0.5f))
                    {
                        aiState = AIState.chasing;
                        animator.SetBool("isAttacking", false);
                    }
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.2f);
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
