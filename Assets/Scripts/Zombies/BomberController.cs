using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
public class BomberController : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    public enum State { idle, chasing, attack, patrol, dead, stunned, pipe, hear };
    public State defaultState;
    [HideInInspector] public State currentState;
    public Animator animator;
    public float attackDistance = 1.0f, chaseDistance = 5.0f;
    private float reachDistance;
    public Vector3[] patrolling;
    private int patrollingIdx = 0;
    public int health;
    private Transform attackTarget; // can be player of zombie (if confused)
    private float chaseSpeed = 2.0f;
    private float patrolSpeed = 0.5f;
    public Transform childTransform;
    private float chaseAngle = 130.0f, attackAngle = 40.0f;
    public float attackCooldownTime = 1;
    bool canAttack = true;
    public int damagePerSec = 5;
    private bool isConfused = false;
    private float stunTimer = 0, confusionTimer = 0, pipeTimer = 10;
    private Vector3 hearedLocation;
    Transform pipePosition;
    public GameObject bomb;

    //                   0        1           2            3            4           5         6                7
    string[] arr = { "isIdle","isPatrol" ,"isChasing","isAttacking","isStunned","isPiped","isReachedPipe","isDying" };

    private void FaceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);
    }
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
        int min = 0;
        for (int i = 0; i < enemies.Count; i++)
            if (Vector3.Distance(((Transform)enemies[i]).position, transform.position) < Vector3.Distance(((Transform)enemies[min]).position, transform.position))
                min = i;
        attackTarget = (Transform)enemies[min];
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackTarget = playerTransform;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        animator.SetBool("isPatrol", defaultState == State.patrol);
        reachDistance = attackDistance + 0.5f;
    }
    public void stun()
    {
        if (currentState == State.dead)
            return;
        currentState = State.stunned;
        navMeshAgent.SetDestination(transform.position);
        SetAnimationFlags(4);
        endConfusion(false);
        stunTimer = 0;
    }
    public void chase(Transform target)
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = chaseSpeed;
        currentState = State.chasing;
        SetAnimationFlags(2);
        navMeshAgent.SetDestination(target.position);
    }
    public virtual void attack()
    {
        if (currentState == State.dead)
            return;
        FaceTarget(attackTarget.position);
        if (attackTarget.tag == "Player")
        {
            PlayerController cont = playerTransform.gameObject.GetComponent<PlayerController>();
            canAttack = false;
            currentState = State.attack;
            SetAnimationFlags(3);
            navMeshAgent.SetDestination(transform.position);
            StartCoroutine(applyDamage(cont));
        }
        else if (attackTarget.tag == "Enemy")
        {
            EnemyContoller cont = attackTarget.gameObject.GetComponent<EnemyContoller>();

            canAttack = false;
            currentState = State.attack;
            SetAnimationFlags(3);
            navMeshAgent.SetDestination(transform.position);
            cont.TakeDamage(damagePerSec);
        }
        StartCoroutine(SetIdleAfterAttack());
        StartCoroutine(resumeAttack());
    }
    void patrol()
    {
        if (currentState == State.dead)
            return;
        navMeshAgent.speed = patrolSpeed;
        SetAnimationFlags(1);
        if (currentState != State.patrol)
        {
            currentState = State.patrol;
            patrollingIdx = 0;
        }

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
        SetAnimationFlags(0);
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
        if (callBacktoDefault)
            backToDefault();
    }
    IEnumerator resumeAttack()
    {
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
    }
    IEnumerator applyDamage(PlayerController cont) //Delayed damage on player for effect
    {
        yield return new WaitForSeconds(0.5f);
        if (health > 0 && currentState == State.attack)
            cont.TakeDamage(damagePerSec);
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
            if (confusionTimer > 1.2f)
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
            if (navMeshAgent.remainingDistance < attackDistance && canAttack)
            {
                attack();
            }
            // calculate distance between zombie and player 
            else if((Vector3.Distance(transform.position, attackTarget.position) < attackDistance))
            {
                navMeshAgent.ResetPath();
                SetAnimationFlags(0);
            }
            else
            {
                // keep chasing
                if (isAlive(attackTarget))
                {
                    navMeshAgent.SetDestination(attackTarget.position);
                    SetAnimationFlags(2);
                }
                else
                    backToDefault();
            }
        }
        else if (currentState == State.attack)
        {
            if (!canSee(reachDistance, attackAngle, attackTarget))
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
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f)
                {
                    SetAnimationFlags(6);
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
        SetAnimationFlags(7);
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
        direction *= rangeDistance;
        float angle = Vector3.Angle(transform.forward, direction);
        Ray ray = new Ray(transform.position + new Vector3(0, 1.0f, 0), direction);
        bool checkDistance = Physics.Raycast(ray, out RaycastHit hit, rangeDistance);
        Debug.DrawRay(transform.position + new Vector3(0, 1.0f, 0), direction, Color.green);
        if (checkDistance && Mathf.Abs(angle) < rangeAngle && hit.transform.position == target.position)
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
        SetAnimationFlags(5);
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
            SetAnimationFlags(2);
        }
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PlayerController>().health > 0)
            chase(other.transform);
    }
    IEnumerator SetIdleAfterAttack()
    {
        yield return new WaitForSeconds(1.5f);
        FaceTarget(attackTarget.position);
       GameObject childGrenade= Instantiate(bomb, new Vector3(transform.position.x, transform.position.y + 1.7f, transform.position.z), transform.rotation);
        childGrenade.transform.parent = gameObject.transform;
        if (currentState==State.attack)
        SetAnimationFlags(0);
    }
    void SetAnimationFlags(int g)
    {
        // kol haga false 
        for (int i = 0; i < arr.Length; i++)
        {
            animator.SetBool(arr[i], i == g);
        }
    }
}
