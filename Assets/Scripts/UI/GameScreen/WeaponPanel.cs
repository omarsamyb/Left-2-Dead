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

    string curWeapon = "";

    void Start(){
        
    }
    void Update()
    {
        setWeaponImage();
        setAmmoCount();
    }

    void setWeaponImage(){
        string weapon = "";
        try
        {
            weapon = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().weaponName;
        }
        catch (System.Exception ex)
        {
            return;
        }
        if(curWeapon != weapon){
            activateImage(weapon);
            curWeapon = weapon;
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
        float bulletsInTheGun = 0;
        float bulletsIHave = 0;
        try
        {
            bulletsInTheGun = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().bulletsInTheGun;
            bulletsIHave = PlayerController.instance.player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().bulletsIHave;
        }
        catch (System.Exception ex)
        {
            return;
        }
        ammoCount.GetComponent<TextMeshProUGUI>().SetText(bulletsInTheGun + " / " + bulletsIHave);
        // textmeshPro.SetText("The first number is {0} and the 2nd is {1:2} and the 3rd is {3:0}.", 4, 6.345f, 3.5f);
        // The text displayed will be:
        // The first number is 4 and the 2nd is 6.35 and the 3rd is 4.
    }
}
