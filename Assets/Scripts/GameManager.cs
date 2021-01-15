using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public bool inRageMode;
    [HideInInspector] public int companionId; // 0 - Ellie, 1 - Zoey, 2 - Louis
    [HideInInspector] public bool inMenu; 
    [HideInInspector] public bool inPickUp; 
    [HideInInspector] public bool isGameOver; 

    public int level;
    public string[] sceneNames;
    public GameObject[] playerStartingPos;
    public GameObject[] compStartingPos;
    public float rescueMissionTime;
    public float timeRemaining;
    public bool timerIsRunning;
    public bool failedRescue;
    public InventoryObject craftable;
    public InventoryObject ammo;
    public InventoryObject ingredient;

    [HideInInspector] public bool finalBossDead = false;
    [SerializeField] public AudioClip bossDeadVOClip;
    public AudioClip[] backgroundMusic;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start(){
        craftable.resetInventory();
        ammo.resetInventory();
        ingredient.resetInventory();

        level = 1;
        rescueMissionTime = 60;
        timeRemaining = rescueMissionTime;
        timerIsRunning = false;
        failedRescue = true;
    }

    private void Update()
    {
        if(inMenu || isGameOver || SceneManager.GetActiveScene().buildIndex==0) // Pause
        {
            Cursor.lockState = CursorLockMode.None;
            if((!inPickUp || isGameOver)&&SceneManager.GetActiveScene().buildIndex!=0){
                Time.timeScale = 0f;
            }
        }
        else // Resume
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                timeRemaining = rescueMissionTime;
                timerIsRunning = false;
                if(failedRescue){
                    isGameOver = true;
                }
            }
        }
    }

    public void LoadNextLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        craftable.resetInventory();
        ammo.resetInventory();
        ingredient.resetInventory();

        level += 1;    
        if(level == 3){
            timerIsRunning = true;
        }
    }

    public string DisplayTime()
    {
        float minutes = Mathf.FloorToInt(timeRemaining / 60);  
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void StopTimer()
    {
        if (level == 3)
            timerIsRunning = !timerIsRunning;
       
    }
    public void FinalBossDead()
    {
        GameObject[] normalEnemy;
        GameObject[] specialEnemy;
        finalBossDead = true;
        normalEnemy = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in normalEnemy)
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(1000);
        }
        specialEnemy = GameObject.FindGameObjectsWithTag("SpecialEnemy");
        foreach (GameObject enemy in specialEnemy)
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(1000);
        }

        StartCoroutine(FinalBossRoutine());
    }
    IEnumerator FinalBossRoutine()
    {
        AudioManager.instance.SetClip("PlayerVoice", bossDeadVOClip);
        AudioManager.instance.Play("PlayerVoice");
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
