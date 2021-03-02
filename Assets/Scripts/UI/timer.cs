using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class timer : MonoBehaviour
{
    public GameObject text;

    void Update()
    {
        if(GameManager.instance.level == 3 && GameManager.instance.timerIsRunning){
            text.GetComponent<TextMeshProUGUI>().SetText(DisplayTime());
            if(GameManager.instance.timeRemaining - 30 <= 0.5){
                text.GetComponent<TextMeshProUGUI>().color = new Color32(255, 128, 0, 255);
            }
            if(GameManager.instance.timeRemaining - 10 <= 0.5){
                text.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
            }
        }
        else{
            if(gameObject.activeSelf){
                gameObject.SetActive(false);
            }
        }
    }
    public string DisplayTime()
    {
        float minutes = Mathf.FloorToInt(GameManager.instance.timeRemaining / 60);
        float seconds = Mathf.FloorToInt(GameManager.instance.timeRemaining % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
