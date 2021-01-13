using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponPanel : MonoBehaviour
{
    public Image assaultRifleImage;
    public Image huntingRifleImage;
    public Image pistolImage;
    public Image shotgunImage;
    public Image submachineGunImage;
    public GameObject ammoCount;
    public GameObject ReloadIcon;
    string curWeapon = "";

    void Start(){
        
    }
    void Update()
    {
        setWeaponImage();
        setAmmoCount();
    }

    void setWeaponImage(){
        if(PlayerController.instance.player.GetComponent<GunInventory>().currentGun != null){
            string weapon = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().weaponName;
            if(curWeapon != weapon){
                activateImage(weapon);
                curWeapon = weapon;
            }
        }
        
    }

    void activateImage(string weapon){
        if(curWeapon == "Assault Rifle")
            assaultRifleImage.gameObject.SetActive(false);
        if(curWeapon == "Hunting Rifle")
            huntingRifleImage.gameObject.SetActive(false);
        if(curWeapon == "Pistol")
            pistolImage.gameObject.SetActive(false);
        if(curWeapon == "Tactical Shotgun")
            shotgunImage.gameObject.SetActive(false);
        if(curWeapon == "Submachine Gun")
            submachineGunImage.gameObject.SetActive(false);

        if(weapon == "Assault Rifle")
            assaultRifleImage.gameObject.SetActive(true);
        else if(weapon == "Hunting Rifle")
            huntingRifleImage.gameObject.SetActive(true);
        else if(weapon == "Pistol")
            pistolImage.gameObject.SetActive(true);
        else if(weapon == "Tactical Shotgun")
            shotgunImage.gameObject.SetActive(true);
        else
            submachineGunImage.gameObject.SetActive(true);
    }

    void setAmmoCount(){
        string weapon = "";
        if(PlayerController.instance.player.GetComponent<GunInventory>().currentGun != null){
            weapon = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().weaponName;
        }
        if(PlayerController.instance.player.GetComponent<GunInventory>().currentGun != null){
            float bulletsInTheGun = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().bulletsInTheGun;
            float amountOfBulletsPerLoad = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().amountOfBulletsPerLoad;
            if(weapon == "Pistol"){
                string bulletsIHave = "∞";
                ammoCount.GetComponent<TextMeshProUGUI>().SetText(bulletsInTheGun + " / " + bulletsIHave);
            }
            else{
                float bulletsIHave = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().bulletsIHave;
                ammoCount.GetComponent<TextMeshProUGUI>().SetText(bulletsInTheGun + " / " + bulletsIHave);
            }
            if (amountOfBulletsPerLoad - bulletsInTheGun > 0){
                ReloadIcon.SetActive(true);
            }
            else{
                ReloadIcon.SetActive(false);
            }
        }
    }
}
