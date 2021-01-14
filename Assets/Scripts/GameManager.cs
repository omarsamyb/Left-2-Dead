using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public bool inRageMode;
    [HideInInspector] public int companionId; // 0 - Ellie, 1 - Zoey, 2 - Louis
    [HideInInspector] public bool inMenu; 
    [HideInInspector] public bool inPickUp; 
    [HideInInspector] public bool isGameOver; 

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
        
    }

    private void Update()
    {
        if(inMenu || isGameOver) // Pause
        {
            Cursor.lockState = CursorLockMode.None;
            if(!inPickUp || isGameOver){
                Time.timeScale = 0f;
            }
        }
        else // Resume
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }
}
