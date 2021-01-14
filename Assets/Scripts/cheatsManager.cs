using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class cheatsManager : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            IncreasePlayerHealth();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            KillAllInfected();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DamageAllInfected();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            FillWeaponsAmmo();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            AddGrenadesToPlayer();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            GenerateAmmoPack();
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            GenerateHealthPack();
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            AddAmmoClipCompanion();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SpawnDifferentInfected();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            SpawnHorde();
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            IncreaseRageMeter();
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ToggleRageMeter();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            GoToNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {   
            FreezeTime();
        }
    }
    public void IncreasePlayerHealth()
    {
        PlayerController.instance.health = PlayerController.instance.health + 30;
        PlayerController.instance.healthBar.SetHealth(PlayerController.instance.health);
    }
    public void KillAllInfected()
    {

    }
    public void DamageAllInfected()
    {
        
    }
    public void FillWeaponsAmmo()
    {

    }
    public void AddGrenadesToPlayer()
    {
        craft.instance.makeBileCheat();
        craft.instance.makeMolotovCheat();
        craft.instance.makePipeCheat();
        craft.instance.makeStunCheat();
    }
    public void GenerateAmmoPack()
    {

    }
    public void GenerateHealthPack()
    {

    }
    public void AddAmmoClipCompanion()
    {
        CompanionController.instance.AddClip();
    }
    public void SpawnDifferentInfected()
    {

    }
    public void SpawnHorde()
    {

    }
    public void IncreaseRageMeter()
    {
        // Rage.instance.ragePoints =  Rage.instance.ragePoints + 10;
    }
    public void ToggleRageMeter()
    {
        GameManager.instance.inRageMode = !GameManager.instance.inRageMode;
    }
    public void GoToNextLevel()
    {

    }
    public void FreezeTime()
    {

    }
}
