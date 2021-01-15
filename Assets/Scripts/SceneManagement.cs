using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneManagement : MonoBehaviour
{

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
            GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Companions/Ellie.prefab", typeof(GameObject));
            Instantiate(resource, transform.position, Quaternion.identity);
        }
        else if(GameManager.instance.companionId == 1)
        {
            GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Companions/Zoey.prefab", typeof(GameObject));
            Instantiate(resource, transform.position, Quaternion.identity);
        }
        else if (GameManager.instance.companionId == 2)
        {
            GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Companions/Louis.prefab", typeof(GameObject));
            Instantiate(resource, transform.position, Quaternion.identity);
        }
    }
}
