using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public class EnemyContoller : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public Transform playerTransform;
    public enum State { idle, chasing, attack, patrol, dead, stunned,pipe };
    public State defaultState;
    [HideInInspector] public State currentState;
    [HideInInspector] public Animator animator;
    public float attackDistance = 1.0f,chaseDistance = 5.0f;
    public Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;
    private Transform attackTarget; // can be player of zombie (if confused)
    private float chaseSpeed=2.0f;
    private float patrolSpeed=0.3f;
    public Transform childTransform;
    private float chaseAngle=130.0f;
    float attackCooldownTime = 1;
    bool canAttack = true;
    int damage = 50;
    Coroutine conf;
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
        if(conf!=null)
            StopCoroutine(conf);
        conf = StartCoroutine(waitForConfusion());
        int random = Random.Range(0,enemies.Count);
        attackTarget = (Transform)enemies[random];
    }
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle",defaultState==State.idle);
    }
    public void stun()
    {
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("isChasing",false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStunned", true);
        animator.SetBool("isIdle", false);
        StopCoroutine(waitForConfusion());
        StartCoroutine(getUnstunned());
    }
    public void chase(Transform target)
    {
        navMeshAgent.speed=chaseSpeed;
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        navMeshAgent.SetDestination(target.position);
    }
    public virtual void attack()
    {
        if(attackTarget.tag=="Player")
        {
            PlayerController cont = playerTransform.gameObject.GetComponent<PlayerController>();  
            if(cont.health>0)
            {
                canAttack = false;
                currentState = State.attack;
                animator.SetBool("isAttacking", true);
                navMeshAgent.SetDestination(transform.position);
                StartCoroutine(applyDamage(cont));
            }
            else
            {
                idle();
            }
        }
        else if(attackTarget.tag=="Enemy")
        {
            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();
            if(cont.health>0)
            {
                canAttack = false;
                currentState = State.attack;
                animator.SetBool("isAttacking", true);
                navMeshAgent.SetDestination(transform.position);
                cont.TakeDamage(damage);
            }
        }
        StartCoroutine(resumeAttack());
    }
    void patrol()
    {
        navMeshAgent.speed=patrolSpeed;
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
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
    void backToDefault()
    {
        if(currentState!=State.dead)
        {
            if(defaultState == State.patrol)
                patrol();
            if(defaultState == State.idle)
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
    IEnumerator waitForConfusion()
    {
        yield return new WaitForSeconds(5);
        if(health>0)
        {
            backToDefault();
            attackTarget = playerTransform;
        }
        
        conf = null;
    }
    IEnumerator getUnstunned()
    {
        yield return new WaitForSeconds(3);
        animator.SetBool("isStunned", false);
        backToDefault();
    }
    IEnumerator pipeExploded()
    {
        yield return new WaitForSeconds(4);
        if(health>0)
        {
            animator.SetBool("isReachedPipe",false);
            animator.SetBool("isPiped", false);
            backToDefault();
        }
    }
    IEnumerator resumeAttack(){
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
        animator.SetBool("isAttacking", false);
    }
    IEnumerator applyDamage(PlayerController cont) //Delayed damage on player for effect
    {
        yield return new WaitForSeconds(0.5f);
        cont.TakeDamage(damage);
    }
    void Update()
    {
        childTransform.position=transform.position;
        if(currentState == State.dead)
            return;
        if(currentState==State.patrol)
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
        else if(currentState==State.chasing){
            if (canSee(attackDistance, 30f,attackTarget) && canAttack)
            {
                attack();
            }
            else
            {
                // keep chasing
                if(isAlive(attackTarget))
                    navMeshAgent.SetDestination(attackTarget.position);
                else
                    backToDefault();
            }
        }
        else if(currentState==State.attack){
            if (!canSee(attackDistance, 30f, attackTarget))
            {
                chase(attackTarget);
            }
            else if(canAttack && isAlive(attackTarget))
            {
                attack();
            }
        }
        else if (currentState == State.pipe)
        {
            if(navMeshAgent.remainingDistance<1f)
            {
                animator.SetBool("isReachedPipe",true);
            }
        }
        else if(currentState == State.idle)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
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
    bool isAlive(Transform target)
    {
        if(target.gameObject.tag=="Player")
        {
            PlayerController cont = target.gameObject.GetComponent<PlayerController>();
            if(cont.health<=0)
                return false;
        }
        else if(target.gameObject.tag=="Enemy")
        {
            EnemyContoller cont = target.gameObject.GetComponent<EnemyContoller>();
            if(cont.health<=0)
                return false;
        }
        return true;
    }
    public bool canSee(float rangeDistance, float rangeAngle,Transform target)
    {
        if(!isAlive(target))
            return false;
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
        if (currentState == State.dead || currentState == State.stunned) return;
        navMeshAgent.SetDestination(grenadePosition.position);
        currentState = State.pipe;
        navMeshAgent.speed= chaseSpeed;
        animator.SetBool("isPiped", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdle", false);
        StopCoroutine(waitForConfusion()); //If i am in bile, ignore bile effect
        StartCoroutine(pipeExploded());
    }
    public string getState()
    {
        return currentState.ToString();
    }
}
