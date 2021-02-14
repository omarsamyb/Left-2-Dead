using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum InfectedState
{
    idle, patrol, chase, attack, distraction, dead
}

[RequireComponent(typeof(NavMeshAgent))]
public class InfectedController : MonoBehaviour
{
    [HideInInspector] public int health;
    [SerializeField] private Collider[] meshColliders;
    private Collider rootCollider;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected float stoppingDistance;
    [SerializeField] protected InfectedState state;
    [SerializeField] private Vector3[] patrolPoints;
    [SerializeField] private float patrolDelayTime;
    protected float patrolSpeed;
    protected float chaseSpeed;
    private float suspiciousWalkSpeed;
    protected int dps;
    protected float attackTime;
    protected float attackDelayTime;
    protected float attackRange;
    protected bool targetType;
    protected Transform target;
    private float distanceToTarget;
    private Vector3 randomPatrolPoint;
    private NavMeshHit navMeshHit;
    private NavMeshPath path;
    private Vector3 faceDirection;
    private LayerMask infectedLayer;
    private bool criticalEvent;
    protected InfectedController attackedInfected;
    // States
    private Coroutine patrolRoutine;
    private bool inPatrolRoutine;
    private WaitForSeconds patrolDelay;
    private int patrolPointIndex;
    private Coroutine chaseRoutine;
    private bool inChaseRoutine;
    private WaitForSeconds chaseDelay;
    protected Coroutine attackRoutine;
    protected bool inAttackRoutine;
    protected WaitForSeconds attackDelay;
    // Detection
    private Coroutine playerVisiblityRoutine;
    private bool inPlayerVisiblityRoutine;
    private WaitForSeconds playerVisiblityDelay;
    private float angleToPlayer;
    private const float visionAngleRef = 45f;
    private float visionAngle;
    private float visionRange;
    private RaycastHit visionHit;
    // Distraction
    private bool resetState;
    private InfectedState preDistractionState;
    private Vector3 preDistractionPosition;
    private Quaternion preDistractionRotation;
    private Quaternion fromRotation;
    private const float rotationSpeed = 1.5f;
    private Coroutine resetPositionRoutine;
    private bool inResetPositionRoutine;
    private Coroutine resetRotationRoutine;
    private bool inResetRotationRoutine;
    private Coroutine noiseRoutine;
    private bool inNoiseRoutine;
    private Coroutine stunRoutine;
    private WaitForSeconds stunTime;
    private bool inStunRoutine;
    private Coroutine pipeRoutine;
    private bool inPipeRoutine;
    private bool pipeExploaded;
    private List<Transform> pipeSources = new List<Transform>();
    private Coroutine explosionRoutine;
    private bool inExplosionRoutine;
    private Coroutine bileRoutine;
    private bool inBileRoutine;
    private const float bileTimerRef = 5f;
    private float bileTimer;
    private Vector3 proximity = new Vector3(10f, 1f, 10f);
    private Coroutine fireRoutine;
    private bool inFireRoutine;
    private const float fireTimerRef = 1.1f;
    private float fireTimer;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        rootCollider = GetComponent<Collider>();
        infectedLayer = 1 << LayerMask.NameToLayer("Enemy");

        patrolDelay = new WaitForSeconds(patrolDelayTime);
        chaseDelay = new WaitForSeconds(0.1f);
        stunTime = new WaitForSeconds(3f);
        playerVisiblityDelay = new WaitForSeconds(0.1f);

        visionAngle = visionAngleRef;
        visionRange = 180f;
        suspiciousWalkSpeed = 1.5f;

        target = PlayerController.instance.transform;
        targetType = true;
    }
    protected virtual void Update()
    {
        TargetCheck();
        StateCheck();
        Vision();
        AttackRange();
        Distraction();
    }
    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if (ConditionsCheck(7))
        {
            if (other.CompareTag("Player"))
                state = InfectedState.chase;
        }
    }

    // States
    private void StateCheck()
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
            case InfectedState.attack:
                Attack();
                break;
        }
    }
    private void Idle()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0))
            animator.SetTrigger("isIdle");
    }
    private void Patrol()
    {
        if (!inPatrolRoutine)
            patrolRoutine = StartCoroutine(PatrolRoutine());
    }
    private IEnumerator PatrolRoutine()
    {
        print(gameObject.name + " Started Patrol Routine");
        inPatrolRoutine = true;
        agent.stoppingDistance = 0.5f;
        agent.speed = patrolSpeed;
        while (true)
        {
            if (patrolPoints.Length == 0)
            {
                randomPatrolPoint = transform.position + Random.insideUnitSphere * 5f;
                if (NavMesh.SamplePosition(randomPatrolPoint, out navMeshHit, 2.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                    agent.SetDestination(navMeshHit.position);
                else
                    continue;
            }
            else
            {
                if (NavMesh.SamplePosition(patrolPoints[patrolPointIndex], out navMeshHit, 5.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                    agent.SetDestination(navMeshHit.position);
                else
                    Debug.LogWarning("Unreachable Patrol Point, please adjust " + gameObject.name + "'s patrol point at index " + patrolPointIndex);
            }
            animator.SetTrigger("isPatroling");
            while (true)
            {
                if (ReachedDestination())
                    break;
                yield return null;
            }
            if(patrolPoints.Length != 0)
                patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Length;
            Idle();
            yield return patrolDelay;
        }
    }
    private void Chase()
    {
        if (!inChaseRoutine && target)
            chaseRoutine = StartCoroutine(ChaseRoutine());
    }
    private IEnumerator ChaseRoutine()
    {
        print(gameObject.name + " Started Chase Routine");
        inChaseRoutine = true;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        while (true)
        {
            try
            {
                if (agent.CalculatePath(target.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Chasing") && !animator.IsInTransition(0))
                        animator.SetTrigger("isChasing");
                    agent.SetPath(path);
                }
                else
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0))
                        animator.SetTrigger("isIdle");
                    Debug.LogWarning("Unreachable target during chase state of " + gameObject.name);
                }
            }
            catch
            {
                print(gameObject.name + " Stopping Chase Routine - target died");
                inChaseRoutine = false;
                yield break;
            }
            yield return chaseDelay;
        }
    }
    protected virtual bool Attack()
    {
        if (inAttackRoutine || !target)
            return false;
        return true;
    }

    // Detection
    private void Vision()
    {
        if (ConditionsCheck(0))
        {
            angleToPlayer = Vector3.Angle((PlayerController.instance.transform.position - transform.position), transform.forward);
            if (angleToPlayer >= -visionAngle && angleToPlayer <= visionAngle)
            {
                playerVisiblityRoutine = StartCoroutine(PlayerVisiblity());
            }
        } 
    } 
    private IEnumerator PlayerVisiblity()
    {
        inPlayerVisiblityRoutine = true;
        if(Physics.Raycast(transform.position, (PlayerController.instance.transform.position - transform.position), out visionHit, visionRange, ~infectedLayer, QueryTriggerInteraction.Ignore))
        {
            if (visionHit.collider.CompareTag("Player"))
            {
                state = InfectedState.chase;
                ResetRoutines("playerVisiblityRoutine");
            }
        }
        yield return playerVisiblityDelay;
        inPlayerVisiblityRoutine = false;
    }
    private void AttackRange()
    {
        if (ConditionsCheck(1))
        {
            if (distanceToTarget <= attackRange && ReachedDestination())
            {
                if (state == InfectedState.chase)
                {
                    if (bileTimer > 0f)
                        ResetRoutines("bileRoutine");
                    else
                        ResetRoutines();
                    state = InfectedState.attack;
                }
            }
            else if(state == InfectedState.attack)
            {
                if (bileTimer > 0f)
                    ResetRoutines("bileRoutine");
                else
                    ResetRoutines();
                state = InfectedState.chase;
            }
        }
    }
    private void TargetCheck()
    {
        if ((targetType && PlayerController.instance.health <= 0) || (!targetType && attackedInfected.health <= 0))
            target = null;
        if (target)
            distanceToTarget = Vector3.SqrMagnitude(target.position - transform.position);
    }

    // Distraction
    private void Distraction()
    {
        if (bileTimer > 0f)
        {
            bileTimer -= Time.deltaTime;
            if (!inBileRoutine)
                Bile(true);
        }
        if(pipeSources.Count > 0)
        {
            pipeSources.RemoveAll(source => !source || source.localScale == Vector3.zero);
            foreach (Transform source in pipeSources)
            {
                if(!inPipeRoutine)
                    Pipe(pipeSources[0], true);
            }
        }
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;
    }
    public void Noise(Vector3 source)
    {
        if (ConditionsCheck(2))
        {
            if (state != InfectedState.distraction)
            {
                preDistractionPosition = transform.position;
                preDistractionRotation = transform.rotation;
                preDistractionState = state;
            }
            ResetRoutines();
            state = InfectedState.distraction;
            noiseRoutine = StartCoroutine(NoiseRoutine(source));
        }
    }
    private IEnumerator NoiseRoutine(Vector3 source)
    {
        print(gameObject.name +" Started Noise Routine");
        inNoiseRoutine = true;
        agent.stoppingDistance = 0.5f;
        agent.speed = suspiciousWalkSpeed;
        if (agent.CalculatePath(source, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("SuspiciousWalking"))
                animator.SetTrigger("isSuspicious");
            agent.SetPath(path);
        }
        else
        {
            Debug.LogWarning("Unreachable target during Noise Routine of " + gameObject.name);
            inNoiseRoutine = false;
            yield break;
        }

        while (true)
        {
            if (ReachedDestination())
                break;
            yield return null;
        }
        visionAngle = 90f;
        animator.SetTrigger("isSuspiciousChecking");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
        visionAngle = visionAngleRef;
        agent.SetDestination(preDistractionPosition);
        while (true)
        {
            if (ReachedDestination())
                break;
            yield return null;
        }
        yield return resetRotationRoutine = StartCoroutine(ResetRotation());
        state = preDistractionState;
        inNoiseRoutine = false;
        print(gameObject.name + " Stopping Noise Routine");
    }
    public void Stun()
    {
        if (ConditionsCheck(3))
        {
            if (state != InfectedState.distraction && bileTimer <= 0f)
                preDistractionState = state;
            ResetRoutines();
            state = InfectedState.distraction;
            stunRoutine = StartCoroutine(StunRoutine());
        }
    }
    private IEnumerator StunRoutine()
    {
        print(gameObject.name + " Started Stun Routine");
        inStunRoutine = true;
        animator.SetTrigger("isStunned");
        yield return stunTime;
        state = preDistractionState;
        inStunRoutine = false;
        print(gameObject.name + " Stopping Stun Routine");
    }
    public void Pipe(Transform source, bool resume = false)
    {
        if (!resume)
            pipeSources.Add(source);
        if (ConditionsCheck(4))
        {
            if (state != InfectedState.distraction)
            {
                preDistractionState = state;
                preDistractionPosition = transform.position;
                preDistractionRotation = transform.rotation;
            }
            ResetRoutines();
            state = InfectedState.distraction;
            pipeRoutine = StartCoroutine(PipeRoutine(source));
        }
    }
    private IEnumerator PipeRoutine(Transform source)
    {
        print(gameObject.name + " Started Pipe Routine");
        inPipeRoutine = true;
        pipeExploaded = false;
        agent.stoppingDistance = 0.5f;
        agent.speed = chaseSpeed;

        if (agent.CalculatePath(source.position, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            animator.SetTrigger("isChasing");
            agent.SetPath(path);
        }
        else
        {
            Debug.LogWarning("Unreachable target during Pipe Routine of " + gameObject.name);
            if (!animator.IsInTransition(0) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                animator.SetTrigger("isIdle");
            inPipeRoutine = false;
            yield break;
        }

        while (true)
        {
            if (ReachedDestination())
                break;
            yield return null;
        }

        animator.SetTrigger("isCheckingPipe");
        yield return new WaitUntil(() => pipeExploaded);

        ResetRoutines("pipeRoutine");
        yield return resetPositionRoutine = StartCoroutine(ResetPosition());
        if (state != InfectedState.dead)
            state = preDistractionState;
        inPipeRoutine = false;
        print(gameObject.name + " Stopping Pipe Routine");
    }
    public void Bile(bool resume=false)
    {
        if (!resume)
            bileTimer = bileTimerRef;
        if (ConditionsCheck(5))
        {
            if (state != InfectedState.distraction)
            {
                preDistractionState = state;
                preDistractionPosition = transform.position;
                preDistractionRotation = transform.rotation;
            }
            ResetRoutines();
            state = InfectedState.distraction;
            bileRoutine = StartCoroutine(BileRoutine());
        }
    }
    private IEnumerator BileRoutine()
    {
        print(gameObject.name + " Started Bile Routine");
        inBileRoutine = true;
        while(bileTimer > 0f)
        {
            Collider[] colliders = Physics.OverlapBox(transform.position + transform.up, proximity, Quaternion.identity, infectedLayer);
            if(colliders.Length > 1)
            {
                foreach (Collider collider in colliders)
                {
                    if(!ReferenceEquals(collider.transform, transform))
                    {
                        attackedInfected = collider.GetComponent<InfectedController>();
                        break;
                    }
                }
                target = attackedInfected.transform;
                targetType = false;
                state = InfectedState.chase;
                yield return new WaitUntil(() => attackedInfected.health <= 0 || bileTimer <= 0f);
                state = InfectedState.distraction;
            }
            else
            {
                if (!animator.IsInTransition(0) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    animator.SetTrigger("isIdle");
                    print("No Infected nearby, retrying...");
                }
                yield return chaseDelay;
                state = InfectedState.distraction;
            }
        }
        ResetRoutines("bileRoutine");
        yield return resetPositionRoutine = StartCoroutine(ResetPosition());
        if (state != InfectedState.dead)
            state = preDistractionState;
        inBileRoutine = false;
        print(gameObject.name + " Stopping Bile Routine");
    }

    // Health
    public void ApplyDamage()
    {
        if (target && state == InfectedState.attack)
        {
            bool targetDied = false;
            if (targetType)
            {
                PlayerController.instance.TakeDamage(Mathf.CeilToInt(dps * attackTime));
                if (PlayerController.instance.health <= 0)
                    targetDied = true;
            }
            else
            {
                attackedInfected.TakeDamage(Mathf.CeilToInt(dps * attackTime), -1);
                if (attackedInfected.health <= 0)
                    targetDied = true;
                else
                {
                    if(attackedInfected.state == InfectedState.idle || attackedInfected.state == InfectedState.patrol)
                    {
                        attackedInfected.bileTimer = bileTimer;
                        attackedInfected.Bile(true);
                    }
                }
            }
            if (targetDied)
                target = null;
            attackTime = 0;
        }
    }
    public void TakeDamage(int points, int type)
    {
        health -= points;
        switch (type)
        {
            // Bullet
            case 0:
                Noise(PlayerController.instance.transform.position);
                if(!animator.IsInTransition(1) && animator.GetCurrentAnimatorStateInfo(1).IsName("Hit"))
                {
                    if (Random.Range(0, 1) == 0)
                        animator.SetTrigger("isHit");
                }
                else
                    animator.SetTrigger("isHit");
                break;
            // Pipe
            case 1:
                criticalEvent = true;
                if (inFireRoutine)
                {
                    inFireRoutine = false;
                    StopCoroutine(fireRoutine);
                }
                if (inExplosionRoutine)
                {
                    inExplosionRoutine = false;
                    StopCoroutine(explosionRoutine);
                }
                explosionRoutine = StartCoroutine(Explosion());
                break;
            // Fire
            case 2:
                criticalEvent = true;
                fireTimer = fireTimerRef;
                if (!inFireRoutine)
                    fireRoutine = StartCoroutine(Fire());
                break;
        }
        if (health <= 0)
            Die();
    }
    private IEnumerator Explosion()
    {
        inExplosionRoutine = true;
        resetState = false;
        if (state != InfectedState.distraction)
        {
            preDistractionState = state;
            preDistractionPosition = transform.position;
            preDistractionRotation = transform.rotation;
            resetState = true;
        }
        state = InfectedState.distraction;
        if (inPipeRoutine)
            ResetRoutines("pipeRoutine");
        else
            ResetRoutines();

        if (health <= 0)
            animator.SetBool("isDamageDead", true);
        RandomRotation();
        animator.SetTrigger("isExploaded");
        yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
        if (state != InfectedState.dead)
        {
            yield return new WaitUntil(() => !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
            if (resetState)
            {
                yield return resetPositionRoutine = StartCoroutine(ResetPosition());
                state = preDistractionState;
            }
        }
        pipeExploaded = true;
        criticalEvent = false;
        inExplosionRoutine = false;
        // TODO: Check the neccessity of this condition
        //if (inPipeRoutine)
        //    ResetRoutines("pipeRoutine");
        //else
        //    ResetRoutines();
    }
    private IEnumerator Fire()
    {
        inFireRoutine = true;
        if (state != InfectedState.distraction)
            preDistractionState = state;
        state = InfectedState.distraction;
        ResetRoutines();
        RandomRotation();
        animator.SetBool("burningMirror", Random.Range(0, 2) == 0? true: false);
        animator.SetFloat("burningMultiplier", Random.Range(0.8f, 1.2f));
        animator.SetTrigger("isBurning");
        while (fireTimer > 0f)
        {
            if(state == InfectedState.dead)
            {
                inFireRoutine = false;
                yield break;
            }
            yield return null;
        }
        if(state != InfectedState.dead)
            state = preDistractionState;
        criticalEvent = false;
        inFireRoutine = false;
        ResetRoutines();
    }
    private void Die()
    {
        animator.SetBool("isDead", true);
        state = InfectedState.dead;
        ResetRoutines();
        rootCollider.enabled = false;
        foreach (Collider collider in meshColliders)
        {
            collider.enabled = false;
        }
        Destroy(gameObject, 6f);
    }

    // Helpers
    private bool ConditionsCheck(int type)
    {
        if (state == InfectedState.dead || criticalEvent)
            return false;
        switch (type)
        {
            // Vision
            case 0:
                if (state == InfectedState.chase || state == InfectedState.attack || inPlayerVisiblityRoutine || distanceToTarget > visionRange || !target)
                    return false;
                if (state == InfectedState.distraction && !inNoiseRoutine)
                    return false;
                break;
            // AttackRange
            case 1:
                if (state != InfectedState.chase && state != InfectedState.attack)
                    return false;
                if (!target)
                    return false;
                break;
            // Noise
            case 2:
                if (state == InfectedState.chase || state == InfectedState.attack || !target)
                    return false;
                if (inStunRoutine || inPipeRoutine || inBileRoutine)
                    return false;
                break;
            // Stun
            case 3:
                break;
            // Pipe
            case 4:
                if (inStunRoutine || inBileRoutine || inPipeRoutine)
                    return false;
                break;
            // Bile
            case 5:
                if (inStunRoutine || inBileRoutine)
                    return false;
                break;
            // Reset Position
            case 6:
                if (preDistractionState != InfectedState.idle && preDistractionState != InfectedState.patrol)
                    return false;
                break;
            // Trigger Enter
            case 7:
                if (state != InfectedState.idle && state != InfectedState.patrol)
                    return false;
                break;
        }
        return true;
    }
    private void ResetRoutines(string exception="")
    {
        agent.updateRotation = true;
        agent.ResetPath();
        if(bileTimer <= 0f && PlayerController.instance.health > 0)
        {
            target = PlayerController.instance.transform;
            targetType = true;
        }

        if(inResetPositionRoutine && exception != "resetPositionRoutine")
        {
            print(gameObject.name + " Stopping Reset Position Routine");
            StopCoroutine(resetPositionRoutine);
            inResetPositionRoutine = false;
        }
        if (inResetRotationRoutine && exception != "resetRotationRoutine")
        {
            print(gameObject.name + " Stopping Reset Rotation Routine");
            StopCoroutine(resetRotationRoutine);
            inResetRotationRoutine = false;
        }
        if (inPlayerVisiblityRoutine && exception != "playerVisiblityRoutine")
        {
            print(gameObject.name + " Stopping Player Visiblity Routine");
            StopCoroutine(playerVisiblityRoutine);
            inPlayerVisiblityRoutine = false;
        }
        if (inPatrolRoutine && exception != "patrolRoutine")
        {
            print(gameObject.name + " Stopping Patrol Routine");
            StopCoroutine(patrolRoutine);
            inPatrolRoutine = false;
        }
        if (inChaseRoutine && exception != "chaseRoutine")
        {
            print(gameObject.name + " Stopping Chase Routine");
            StopCoroutine(chaseRoutine);
            inChaseRoutine = false;
        }
        if (inAttackRoutine && exception != "attackRoutine")
        {
            print(gameObject.name + " Stopping Attack Routine");
            StopCoroutine(attackRoutine);
            inAttackRoutine = false;
        }
        if (inNoiseRoutine && exception != "noiseRoutine")
        {
            print(gameObject.name + " Stopping Noise Routine");
            StopCoroutine(noiseRoutine);
            inNoiseRoutine = false;
        }
        if (inStunRoutine && exception != "stunRoutine")
        {
            print(gameObject.name + " Stopping Stun Routine");
            StopCoroutine(stunRoutine);
            inStunRoutine = false;
        }
        if (inPipeRoutine && exception != "pipeRoutine")
        {
            print(gameObject.name + " Stopping Pipe Routine");
            StopCoroutine(pipeRoutine);
            inPipeRoutine = false;
        }
        if (inBileRoutine && exception != "bileRoutine")
        {
            print(gameObject.name + " Stopping Bile Routine");
            StopCoroutine(bileRoutine);
            inBileRoutine = false;
        }
    }
    private IEnumerator ResetPosition()
    {
        if (ConditionsCheck(6))
        {
            print(gameObject.name + " Starting Reset Position Routine");
            inResetPositionRoutine = true;
            agent.stoppingDistance = 0.5f;
            agent.speed = patrolSpeed;
            agent.updateRotation = true;
            animator.SetTrigger("isPatroling");
            agent.SetDestination(preDistractionPosition);
            while (true)
            {
                if (ReachedDestination())
                    break;
                yield return null;
            }
            yield return resetRotationRoutine = StartCoroutine(ResetRotation());

            print(gameObject.name + " Stopping Reset Position Routine");
            inResetPositionRoutine = false;
        }
    }
    private IEnumerator ResetRotation()
    {
        inResetRotationRoutine = true;
        fromRotation = transform.rotation;
        for (float t = 0f; t < 1f; t += Time.deltaTime * rotationSpeed)
        {
            transform.rotation = Quaternion.Lerp(fromRotation, preDistractionRotation, t);
            yield return null;
        }
        inResetRotationRoutine = false;
    }
    private bool ReachedDestination()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.1f)
            {
                agent.ResetPath();
                return true;
            }
        }
        return false;
    }
    protected void FaceTarget()
    {
        faceDirection = target.position - transform.position;
        faceDirection.y = 0f;
        transform.rotation = Quaternion.LookRotation(faceDirection);
    }
    private void RandomRotation()
    {
        if (Random.Range(0, 2) == 0)
            transform.Rotate(Vector3.up, Random.Range(0, 40));
        else
            transform.Rotate(Vector3.up, -Random.Range(0, 40));
    }
}
