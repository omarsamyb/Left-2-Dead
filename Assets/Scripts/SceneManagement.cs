using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneManagement : MonoBehaviour
{
    public GameObject elliePrefab;
    public GameObject zoeyPrefab;
    public GameObject louisPrefab;
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.instance.companionId == 0)
        {
            GameObject resource = elliePrefab;
            Instantiate(resource, transform.position, Quaternion.identity);
        }
        else if(GameManager.instance.companionId == 1)
        {
            GameObject resource = zoeyPrefab;
            Instantiate(resource, transform.position, Quaternion.identity);
        }
        else if (GameManager.instance.companionId == 2)
        {
            GameObject resource = louisPrefab;
            Instantiate(resource, transform.position, Quaternion.identity);
        }
    }
}
