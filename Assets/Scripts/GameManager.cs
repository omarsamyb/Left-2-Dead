using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public bool inRageMode;
    [HideInInspector] public int companionId; // 0 - Ellie, 1 - Zoey, 2 - Louis
    [HideInInspector] public bool inMenu; 
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
}
