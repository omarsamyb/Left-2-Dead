using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Spitter : EnemyContoller
{
    bool alreadyAttacked;


    [HideInInspector]public Vector3 walkPoint;
    bool walkPointSet;
    [HideInInspector]public float walkPointRange;
    LayerMask whatisGround, whatisPlayer;
    [HideInInspector]public bool playerInSightRange, playerInAttackRange;
    
    private void Start()
    {
        // getting player position
        playerTransform = PlayerController.instance.player.transform;
        // setting attack target position
        attackTarget = playerTransform;
        // setting current State of Spitter
        defaultState = State.patrol;
        currentState = defaultState;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator.SetBool("isIdle", defaultState == State.idle);
        reachDistance = attackDistance + 0.5f;
        healthBar.SetMaxHealth(health);
        audioSource = GetComponent<AudioSource>();
        // getting player position
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        walkPointRange = 5.0f;
        whatisPlayer = 1 << LayerMask.NameToLayer("Player");
        whatisGround = 1 << LayerMask.NameToLayer("World");
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = false;
        
    }

    private void Update()
    {
        //Setting position of Spitter model
        childTransform.position = transform.position;
        //-----------------------------------------------------------------
        //Checking if Spitter is somehow looking at the player
        //------------------------------------------------------------------
        Vector3 dirFromAtoB = (playerTransform.position - transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, transform.forward);
        //---------------------Pipe Gernade----------------------

        if (pipeTimer < 4)
            pipeTimer = pipeTimer + Time.deltaTime;

        if (currentState == State.dead || isPinned)
            return;
        else if (currentState == State.pipe)
        {
            if (pipeTimer > 4.0f)
                pipeExploded(true);
            else
            {
                navMeshAgent.SetDestination(pipePosition.position);
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f)
                {
                    animator.SetBool("isReachedPipe", true);
                    navMeshAgent.ResetPath();
                }
            }
        }

        //--------------------------------------Stun Grenad------------------------------------------

        else if (currentState == State.stunned)
        {
            stunTimer = stunTimer + Time.deltaTime;
            if (stunTimer > 3.0f)
                endStun(true);
        }
        //-----------------------------------------------------------------------------------------
    
        //-----------------Confusion(Bile Gernade)---------------------------
        
        else if (isConfused)
        {
            confusionTimer += Time.deltaTime;
            if (confusionTimer > 1.2f)
            {
                endConfusion(true);
            }
        }

        //------------------------------Ask about hear fire----------------------------------------------------

        else if (currentState == State.hear)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
            else if (Vector3.Distance(hearedLocation, transform.position) < navMeshAgent.stoppingDistance)
                backToDefault();
        }

        //-----------------------------------------------------------------------------
        //check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, chaseDistance, whatisPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackDistance, whatisPlayer);


        if (!playerInSightRange && !playerInAttackRange)
        {
            patrol();
        }
        if (playerInSightRange && !playerInAttackRange)
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
        if (playerInSightRange && playerInAttackRange && !alreadyAttacked)
        {

            if (canSee(chaseDistance, chaseAngle, attackTarget))
            {
                attack();

            }
            else
            {
                patrol();
            }
        }
        //--------------Chasing and patroling---------------------------------
        /*
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
          
        */
        //---------------------------------------------------------------

        /*
        else if (currentState == State.chasing)
        {
            if (navMeshAgent.remainingDistance < attackDistance && canAttack)
            {
                attack();
            }
            else
            {
                // keep chasing
                if (isAlive(attackTarget))
                {
                    transform.LookAt(attackTarget);
                    if (Vector3.Distance(curGoToDestination, attackTarget.position) > distanceToUpdateDestination)//Don't update if unecessary
                    {
                        curGoToDestination = attackTarget.position;
                        navMeshAgent.SetDestination(curGoToDestination);
                    }
                }
                else
                    backToDefault();
            }
        }
        */
        //--------------------------------------------------------
        /*
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
        */
        //--------------------------------------------------------------------------
        /*
        else if (currentState == State.idle)
        {
            if (canSee(chaseDistance, chaseAngle, attackTarget))
                chase(attackTarget);
        }
        */


    }
    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        float maxDistance = 2f;
        if (Physics.Raycast(walkPoint, -transform.up, maxDistance, whatisGround))
            walkPointSet = true;
    }
    public override void patrol()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) {
            navMeshAgent.speed = patrolSpeed;
            navMeshAgent.SetDestination(walkPoint);
            currentState = State.patrol;
            turnAgentWhileWalking(walkPoint);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isChasing", false);



        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    public override void chase(Transform target)
    {
        currentState = State.chasing;
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", false);
        navMeshAgent.speed = chaseSpeed;
        transform.LookAt(playerTransform);
        //turnAgentWhileWalking(player.position);
        navMeshAgent.SetDestination(playerTransform.position);
    }


    public void turnAgentWhileWalking(Vector3 point)
    {
        navMeshAgent.stoppingDistance = 0.5f;
        Quaternion newRotation = Quaternion.LookRotation(point - transform.position);
        newRotation.x = 0f;
        newRotation.z = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 1);
    }
    public override void attack()
    {
        //make sure enemy doesn't move
        navMeshAgent.SetDestination(transform.position);
        turnAgentWhileWalking(playerTransform.position);
        transform.LookAt(attackTarget);
        if (!alreadyAttacked)
        {
            // attak code here
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", true);
            currentState = State.attack;
            ///



            alreadyAttacked = true;
            //Invoke(nameof(ResetAttack), attackCooldownTime);
            StartCoroutine(setIdle());
            StartCoroutine(ResetAttack());

        }
    }
    private IEnumerator setIdle()
    {
        yield return new WaitForSeconds(2.8f);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle",true);
        currentState = State.idle;

    }
    private IEnumerator  ResetAttack()
    {
        yield return new WaitForSeconds(attackCooldownTime);
        alreadyAttacked = false;
        animator.SetBool("isIdle", false);

    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }

}
