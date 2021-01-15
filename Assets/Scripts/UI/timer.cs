using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class timer : MonoBehaviour
{
    public GameObject text;

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.level == 3 && GameManager.instance.timerIsRunning){
            text.GetComponent<TextMeshProUGUI>().SetText(GameManager.instance.DisplayTime());
            if(GameManager.instance.timeRemaining - 30 <= 0.5){
                text.GetComponent<TextMeshProUGUI>().color = new Color32(255, 128, 0, 255);
            }
            if(GameManager.instance.timeRemaining - 10 <= 0.5){
                text.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
            }
            print(GameManager.instance.DisplayTime());
        }
        else{
            if(gameObject.activeSelf){
                gameObject.SetActive(false);
            }
        }
    }
}
