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

    bool paused;
    float savedTimeScale;
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
        paused = false;
    }
    private void Update()
    {
        if ((inMenu || isGameOver) && !paused) // Pause
        {
            AudioManager.instance.Play("MenuBackgroundMusic");
            paused = true;
            AudioListener.pause = true;
            UnlockCursor();
            PlayerController.instance.HideEffects();
            if(inMenu ){
                savedTimeScale = Time.timeScale;
            }
            if (isGameOver)
            {
                Time.timeScale = 0f;
            }
            Time.timeScale = 0f;
        }
        else if(!inMenu && !isGameOver && paused)
        {
            AudioManager.instance.Stop("MenuBackgroundMusic");
            AudioListener.pause = false;
            paused = false;
            LockCursor();
            PlayerController.instance.RestoreEffects();
            Time.timeScale = savedTimeScale;
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
            enemy.GetComponent<InfectedController>().TakeDamage(1000, -1);
        }
        specialEnemy = GameObject.FindGameObjectsWithTag("SpecialEnemy");
        foreach (GameObject enemy in specialEnemy)
        {
            enemy.GetComponent<InfectedController>().TakeDamage(1000, -1);
        }
        StartCoroutine(FinalBossRoutine());
    }
    IEnumerator FinalBossRoutine()
    {
        AudioManager.instance.SetClip("PlayerVoice", bossDeadVOClip);
        AudioManager.instance.Play("PlayerVoice");
        yield return new WaitForSeconds(5f);
        LoadNextLevel();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
