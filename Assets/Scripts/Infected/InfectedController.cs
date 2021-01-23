using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum InfectedState
{
    idle, patrol, chase, attack, distraction
}

[RequireComponent(typeof(NavMeshAgent))]
public class InfectedController : MonoBehaviour
{
    public int health;
    private Animator animator;
    private NavMeshAgent agent;
    [SerializeField] private InfectedState state;
    [SerializeField] private Vector3[] patrolPoints;
    [SerializeField] private float patrolDelayTime;
    private float patrolSpeed;
    private float chaseSpeed;
    protected float attackDelayTime;
    protected bool targetType;
    protected Transform target;

    private IEnumerator patrolRoutine;
    private bool inPatrolRoutine;
    private WaitForSeconds patrolDelay;
    private int patrolPointIndex;
    private IEnumerator chaseRoutine;
    private bool inChaseRoutine;
    private WaitForSeconds chaseDelay;
    protected IEnumerator attackRoutine;
    protected bool inAttackRoutine;
    protected WaitForSeconds attackDelay;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        patrolRoutine = PatrolRoutine();
        patrolDelay = new WaitForSeconds(patrolDelayTime);
        chaseRoutine = ChaseRoutine();
        chaseDelay = new WaitForSeconds(0.1f);
    }

    void Update()
    {
        switch (state)
        {
            case InfectedState.idle:
                Idle();
                break;
            case InfectedState.patrol:
                Patrol();
                break;
            case InfectedState.chase:
                Chase();
                break;
        }
    }

    // States
    private void Idle()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            animator.SetTrigger("isIdle");
    }
    private void Patrol()
    {
        if (!inPatrolRoutine)
            StartCoroutine(patrolRoutine);
    }
    private IEnumerator PatrolRoutine()
    {
        inPatrolRoutine = true;
        agent.stoppingDistance = 0.5f;
        agent.speed = patrolSpeed;
        while (true)
        {
            agent.SetDestination(patrolPoints[patrolPointIndex]);
            animator.SetTrigger("isPatroling");
            while (true)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude <= 1f)
                    {
                        break;
                    }
                }
                yield return null;
            }
            Idle();
            yield return patrolDelay;
            patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Length;
        }
    }
    private void Chase()
    {
        if (!inChaseRoutine)
            StartCoroutine(chaseRoutine);
    }
    private IEnumerator ChaseRoutine()
    {
        inChaseRoutine = true;
        agent.speed = chaseSpeed;
        while (true)
        {
            agent.SetDestination(target.position);
            yield return chaseDelay;
        }
    }
    protected virtual void Attack()
    {
        if (!inAttackRoutine)
            StartCoroutine(attackRoutine);
    }
    private void ResetRoutines()
    {
        StopCoroutine(patrolRoutine);
        StopCoroutine(chaseRoutine);
        inPatrolRoutine = false;
        inChaseRoutine = false;
    }

    // Health
    public void TakeDamage(int points)
    {
        health -= points;
        if (health <= 0)
            Die();
    }
    private void Die()
    {
        animator.SetTrigger("isDead");
        Destroy(this, 6f);
    }
}
