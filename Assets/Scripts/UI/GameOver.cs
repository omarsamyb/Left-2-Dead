using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private string startgame;


    private void Start()
    {
        startgame = "Level1";
    }
    public void RestartGame(){
        SceneManager.LoadScene(startgame);
    }
    public void QuitGame(){
        Application.Quit();
    }
}
