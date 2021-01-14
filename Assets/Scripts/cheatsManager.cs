using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class cheatsManager : MonoBehaviour
{
    public GameObject healthPack;
    public GameObject ammoPack;
    public GameObject ammoPack2;
    public GameObject ammoPack3;

    public Transform player;
    private GameObject[] normalEnemy;
    private GameObject[] specialEnemy;


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
        normalEnemy = GameObject.FindGameObjectsWithTag("Enemy");
        specialEnemy = GameObject.FindGameObjectsWithTag("SpecialEnemy");
        foreach(GameObject enemy in normalEnemy) 
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(1000);
        }
        foreach(GameObject enemy in specialEnemy) 
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(1000);
        }

    }
    public void DamageAllInfected()
    {
        normalEnemy = GameObject.FindGameObjectsWithTag("Enemy");
        specialEnemy = GameObject.FindGameObjectsWithTag("SpecialEnemy");
        foreach(GameObject enemy in normalEnemy) 
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(10);
        }
        foreach(GameObject enemy in specialEnemy) 
        {
            enemy.GetComponent<EnemyContoller>().TakeDamage(10);
        }
    }
    public void FillWeaponsAmmo()
    {
        PlayerController.instance.GetComponent<PlayerInventory>().pickUpAmmoCheat();
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
        Instantiate(ammoPack, new Vector3(player.transform.position.x+2, player.transform.position.y+1, player.transform.position.z + 3), Quaternion.identity);
        Instantiate(ammoPack2, new Vector3(player.transform.position.x -2, player.transform.position.y+1, player.transform.position.z + 3), Quaternion.identity);
        Instantiate(ammoPack3, new Vector3(player.transform.position.x + 1, player.transform.position.y+1, player.transform.position.z + 3), Quaternion.identity);

    }
    public void GenerateHealthPack()
    {
        Instantiate(healthPack, new Vector3(player.transform.position.x+4, player.transform.position.y+1, player.transform.position.z + 4), Quaternion.identity);
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
        PlayerController.instance.GetComponent<Rage>().rageCheats();
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
