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
    public InventoryObject ingredientInventory;
    // alcohol  bile  canister  gunpowder  rag  sugar
    //    0      1       2         3        4     5

    public InventoryObject CraftableInventory;
    // bile  molotov  pipe  stun  healthpack
    //  0       1      2     3        4
    private Transform player;
    private GameObject[] normalEnemy;
    private GameObject[] specialEnemy;
    private bool toggleRage = false;

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
        makeBileCheat();
        makeMolotovCheat();
        makePipeCheat();
        makeStunCheat();
    }
    public void GenerateAmmoPack()
    {
        player =  PlayerController.instance.player.transform;
        Instantiate(ammoPack, new Vector3(player.position.x+2, player.position.y+1, player.position.z + 3), Quaternion.identity);
        Instantiate(ammoPack2, new Vector3(player.position.x -2, player.position.y+1, player.position.z + 3), Quaternion.identity);
        Instantiate(ammoPack3, new Vector3(player.position.x + 1, player.position.y+1, player.position.z + 3), Quaternion.identity);

    }
    public void GenerateHealthPack()
    {
        player =  PlayerController.instance.player.transform;
        Instantiate(healthPack, new Vector3(player.position.x+4, player.position.y+1, player.position.z + 4), Quaternion.identity);
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
        PlayerController.instance.player.GetComponent<Rage>().ragePoints += 10;
        PlayerController.instance.player.GetComponent<Rage>().rageReset = 3f;
        PlayerController.instance.player.GetComponent<Rage>().rageCheats();
        if(PlayerController.instance.player.GetComponent<Rage>().ragePoints >=100)
        {
            PlayerController.instance.player.GetComponent<Rage>().canActivate = true;
        }
    }
    public void ToggleRageMeter()
    {
        toggleRage = !toggleRage;
        if(toggleRage){
            PlayerController.instance.player.GetComponent<Rage>().ragePoints = 100;
            PlayerController.instance.player.GetComponent<Rage>().rageCheats();
            PlayerController.instance.player.GetComponent<Rage>().canActivate = true;
        }
        else
        {
            PlayerController.instance.player.GetComponent<Rage>().canActivate = false;
            PlayerController.instance.player.GetComponent<Rage>().rageReset = 0;
            GameManager.instance.inRageMode = false;


        }
    }
    public void GoToNextLevel()
    {

    }
    public void FreezeTime()
    {

    }
    public void makeBileCheat(){
        CraftableInventory.container[0].addAmount(1);
        PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Bile Bomb");
    }
    public void makeMolotovCheat(){
        CraftableInventory.container[1].addAmount(1);
        PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Molotov Cocktail");
    }
    public void makePipeCheat(){
        CraftableInventory.container[2].addAmount(1);
        PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Pipe Bomb");
    }
    public void makeStunCheat(){
        CraftableInventory.container[3].addAmount(1);
        PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Stun Grenade");
    }
}
