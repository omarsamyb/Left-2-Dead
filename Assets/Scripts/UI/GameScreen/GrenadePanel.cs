using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GrenadePanel : MonoBehaviour
{
    public GameObject bileImage;
    public GameObject molotovImage;
    public GameObject pipeImage;
    public GameObject stunImage;
    public GameObject healthPackImage;

    public GameObject changeIcon;
    public GameObject throwIcon;
    public GameObject grenadeCount;
    string curGrenade = "";

    void Update()
    {
        setGrenade();
    }

    void setGrenade(){
        int currentGrenadeCounter = PlayerController.instance.player.GetComponent<GunInventory>().currentGrenadeCounter;
        List<string> grenadesIHave = PlayerController.instance.player.GetComponent<GunInventory>().grenadesIHave;
        List<int> grenadesCounter = PlayerController.instance.player.GetComponent<GunInventory>().grenadesCounter;
        
        if(grenadesIHave.Count != 0){
            string grenade = grenadesIHave[currentGrenadeCounter];
            if(curGrenade != grenade){
                activateImage(grenade);
                curGrenade = grenade;
            }
            if(grenadesIHave.Count == 1){
                changeIcon.SetActive(false);
            }
            else{
                changeIcon.SetActive(true);
            }
            setGrenadeCount(grenadesCounter[currentGrenadeCounter]);
        }
        else{
            deactivateImages();
        }
    }

    void activateImage(string grenade){
        if(curGrenade == "Bile Bomb")
            bileImage.gameObject.SetActive(false);
        if(curGrenade == "Molotov Cocktail")
            molotovImage.gameObject.SetActive(false);
        if(curGrenade == "Pipe Bomb")
            pipeImage.gameObject.SetActive(false);
        if(curGrenade == "Stun Grenade")
            stunImage.gameObject.SetActive(false);

        if(grenade == "Bile Bomb")
            bileImage.gameObject.SetActive(true);
        else if(grenade == "Molotov Cocktail")
            molotovImage.gameObject.SetActive(true);
        else if(grenade == "Pipe Bomb")
            pipeImage.gameObject.SetActive(true);
        else
            stunImage.gameObject.SetActive(true);
    }

    void deactivateImages(){
        bileImage.gameObject.SetActive(false);
        molotovImage.gameObject.SetActive(false);
        pipeImage.gameObject.SetActive(false);
        stunImage.gameObject.SetActive(false);
        healthPackImage.gameObject.SetActive(false);
        grenadeCount.gameObject.SetActive(false);
    }

    void setGrenadeCount(int count){
        if(!grenadeCount.gameObject.activeSelf)
            grenadeCount.gameObject.SetActive(true);
        grenadeCount.GetComponent<TextMeshProUGUI>().SetText(count+"");
    }
}
