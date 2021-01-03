using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
public class companionController : MonoBehaviour
{
        public static companionController instance;
    // Start is called before the first frame update
        private NavMeshAgent agent;
        public Transform target;
        public GameObject player;
        private EnemyContoller closestEnemy;
        private EnemyContoller closestEnemy2;
        public EnemyContoller chosenEnemy;
        private Animator animator;
        public CompanionData Data;
        public int chooseCompanion;
        private int choosenAmmo;
        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            animator = GetComponent<Animator>();
            if(chooseCompanion == 1){ // Ellie
                Data.MaxClips = 3;
                Data.weapon = "Pistol";
                choosenAmmo = Data.pistolAmmo;
                Data.ammo = Data.pistolAmmo;
            }
            else if(chooseCompanion == 2){ // Zoey
                Data.MaxClips = 5;
                Data.weapon = "Hunting Rifle";
                choosenAmmo = Data.huntingRifleAmmo;
                Data.ammo = Data.huntingRifleAmmo;
            }
            else if(chooseCompanion==3){ // Louis
                Data.MaxClips = 4;
                Data.weapon = "Assault Rifle";
                choosenAmmo = Data.assultRifleAmmo;
                Data.ammo = Data.assultRifleAmmo;
            }
        }
        private void Update()
        {
            if (target != null)
                agent.SetDestination(target.position);
            if(agent.remainingDistance > agent.stoppingDistance){
                animator.SetBool("isMoving", true);
            }
            else{
                animator.SetBool("isMoving", false);

            }
            FindClosestEnemy();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(FaceTarget());
            }
        }
        public IEnumerator FaceTarget()
        {
            //get difference of the rotation of the player and gameObjects position
            Vector3 direction = (chosenEnemy.transform.position - transform.position).normalized;
            //set lookRotation to the x and y of the player
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            while((Quaternion.Angle(transform.rotation, lookRotation) > 0.01f)){
                //apply rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
            yield return null;
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }
        public virtual void Ability()
        {
            Debug.Log("Current Ability is " + transform.name);
        }
        public void FindClosestEnemy(){
            float distanceToClosestEnemy = Mathf.Infinity;
            float distanceToClosestEnemy2 = Mathf.Infinity;
            closestEnemy = null;
            closestEnemy2 = null;
            chosenEnemy = null;
            // Get all normal enemies and get their closest and get all special enemies and get closest compare the two if equal then special if not then closest
            EnemyContoller[] allEnemies = GameObject.FindObjectsOfType<EnemyContoller>();
            foreach (EnemyContoller currentEnemy in allEnemies){
                if(currentEnemy.tag == "Enemy"){
                    float distanceToEnemy = (currentEnemy.transform.position - player.transform.position).sqrMagnitude;
                    if(distanceToEnemy < distanceToClosestEnemy){
                        distanceToClosestEnemy = distanceToEnemy;
                        closestEnemy = currentEnemy;
                    }
                }
                // Special
                else if (currentEnemy.tag == "SpecialEnemy"){
                    float distanceToEnemy2 = (currentEnemy.transform.position - player.transform.position).sqrMagnitude;
                    if(distanceToEnemy2 < distanceToClosestEnemy2){
                        distanceToClosestEnemy2 = distanceToEnemy2;
                        closestEnemy2 = currentEnemy;
                    }
                }
            }
            // Compare Special To Normal Enemy
            // Case Equal
            float distanceToNormalEnemy = (closestEnemy.transform.position - player.transform.position).sqrMagnitude;
            float distanceToSpecialEnemy = (closestEnemy2.transform.position - player.transform.position).sqrMagnitude;

            if(distanceToNormalEnemy<distanceToSpecialEnemy){ // Normal Enemy
                chosenEnemy = closestEnemy;
            }
            else if (distanceToNormalEnemy>distanceToSpecialEnemy)// Special
            {
                chosenEnemy = closestEnemy2;
            }
            else{//Special
                chosenEnemy = closestEnemy2;
            }
            Debug.DrawLine(player.transform.position,closestEnemy.transform.position);
            Debug.DrawLine(player.transform.position,closestEnemy2.transform.position);

    }
}
