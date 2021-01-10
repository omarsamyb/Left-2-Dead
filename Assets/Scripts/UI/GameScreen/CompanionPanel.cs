using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompanionPanel : MonoBehaviour
{   
    public GameObject zoeyImage;
    public GameObject louisImage;
    public GameObject ellieImage;

    public GameObject bulletCount;
    public GameObject clipCount;

    bool companionSet = false;

    void Update()
    {
        setAmmoCount();
        setCompanion();
    }

    void setAmmoCount(){
        int bulletsIHave = CompanionController.instance.bulletsIHave;
        int currentClips = CompanionController.instance.currentClips;
        bulletCount.GetComponent<TextMeshProUGUI>().SetText("X " + bulletsIHave);
        clipCount.GetComponent<TextMeshProUGUI>().SetText("X " + currentClips);
    }
    void setCompanion(){
        if(!companionSet){
            int companionId = GameManager.instance.companionId;

            if(companionId == 0){
                ellieImage.SetActive(true);
                zoeyImage.SetActive(false);
                louisImage.SetActive(false);
                companionSet = true;
            }
            else if(companionId == 1){
                ellieImage.SetActive(false);
                zoeyImage.SetActive(true);
                louisImage.SetActive(false);
                companionSet = true;
            }
            else if(companionId == 2){
                ellieImage.SetActive(false);
                zoeyImage.SetActive(false);
                louisImage.SetActive(true);
                companionSet = true;
            }
        }
    }
}
