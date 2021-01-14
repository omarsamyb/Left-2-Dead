using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class craft : MonoBehaviour
{
    public static craft instance;
    public InventoryObject ingredientInventory;
    // alcohol  bile  canister  gunpowder  rag  sugar
    //    0      1       2         3        4     5

    public InventoryObject CraftableInventory;
    // bile  molotov  pipe  stun  healthpack
    //  0       1      2     3        4
    
    public ParticleSystem succParticle;
    public ParticleSystem failParticle;

    public GameObject bileTarget;
    public GameObject molotovTarget;
    public GameObject pipeTarget;
    public GameObject stunTarget;
    public GameObject healthTarget;

    void Start(){

    }

    public void makeBile(){
        if(ingredientInventory.container[1].amount >= 1 && ingredientInventory.container[2].amount >= 1 && ingredientInventory.container[3].amount >= 1 && CraftableInventory.container[0].amount < 1){
            ingredientInventory.container[1].addAmount(-1);
            ingredientInventory.container[2].addAmount(-1);
            ingredientInventory.container[3].addAmount(-1);
            StartCoroutine(succORfail(succParticle, bileTarget));
            CraftableInventory.container[0].addAmount(1);
            PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Bile Bomb");
        }
        else{
            StartCoroutine(succORfail(failParticle, bileTarget));
        }
    }
    public void makeMolotov(){
        if(ingredientInventory.container[0].amount >= 2 && ingredientInventory.container[4].amount >= 2 && CraftableInventory.container[1].amount < 3){
            ingredientInventory.container[0].addAmount(-2);
            ingredientInventory.container[5].addAmount(-2);
            StartCoroutine(succORfail(succParticle, molotovTarget));
            CraftableInventory.container[1].addAmount(1);
            PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Molotov Cocktail");
        }
        else{
            StartCoroutine(succORfail(failParticle, molotovTarget));
        }
    }
    public void makePipe(){
        if(ingredientInventory.container[0].amount >= 1 && ingredientInventory.container[2].amount >= 1 && ingredientInventory.container[3].amount >= 1 && CraftableInventory.container[2].amount < 2){
            ingredientInventory.container[0].addAmount(-1);
            ingredientInventory.container[2].addAmount(-1);
            ingredientInventory.container[3].addAmount(-1);
            StartCoroutine(succORfail(succParticle, pipeTarget));
            CraftableInventory.container[2].addAmount(1);
            PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Pipe Bomb");
        }
        else{
            StartCoroutine(succORfail(failParticle, pipeTarget));
        }
    }
    public void makeStun(){
        if(ingredientInventory.container[3].amount >= 2 && ingredientInventory.container[5].amount >= 1 && CraftableInventory.container[3].amount < 2){
            ingredientInventory.container[3].addAmount(-2);
            ingredientInventory.container[5].addAmount(-1);
            StartCoroutine(succORfail(succParticle, stunTarget));
            CraftableInventory.container[3].addAmount(1);
            PlayerController.instance.player.GetComponent<GunInventory>().AddGrenade("Stun Grenade");
        }
        else{
            StartCoroutine(succORfail(failParticle, stunTarget));
        }
    }
    public void makeHealth(){
        if(ingredientInventory.container[0].amount >= 2 && ingredientInventory.container[4].amount >= 2){
            ingredientInventory.container[0].addAmount(-2);
            ingredientInventory.container[4].addAmount(-2);
            StartCoroutine(succORfail(succParticle, healthTarget));
            CraftableInventory.container[4].addAmount(1);
        }
        else{
            StartCoroutine(succORfail(failParticle, healthTarget));
        }
    }

    IEnumerator succORfail(ParticleSystem p, GameObject target)
    {
        p.transform.position = target.transform.position;
        if(!p.isPlaying)
        {
            p.Play();
        }
        yield return new WaitForSecondsRealtime(1);
        if(p.isPlaying)
        {
            p.Stop();
        }
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
