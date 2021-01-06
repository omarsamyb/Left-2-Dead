using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class craft : MonoBehaviour
{
    public InventoryObject ingredientInventory;
    // alcohol  bile  canister  gunpowder  rag  sugar
    //    0      1       2         3        4     5

    public InventoryObject CraftableInventory;
    // bile  molotov  pipe  stun  healthpack
    //  0       1      2     3        4
    // Start is called before the first frame update
    
    void Start(){
        var x = ingredientInventory.container[0].amount;
        print(x);
        ingredientInventory.container[0].addAmount(-1);
        
        x =ingredientInventory.container[0].amount; 
        print(x);
    }

    public void makeBile(){
        if(ingredientInventory.container[1].amount >= 1 && ingredientInventory.container[2].amount >= 1 && ingredientInventory.container[3].amount >= 1){
            ingredientInventory.container[1].addAmount(-1);
            ingredientInventory.container[2].addAmount(-1);
            ingredientInventory.container[3].addAmount(-1);
            succ();
            CraftableInventory.container[0].addAmount(1);
        }
        else{
            fail();
        }
    }
    public void makeMolotov(){
        if(ingredientInventory.container[0].amount >= 2 && ingredientInventory.container[4].amount >= 2){
            ingredientInventory.container[0].addAmount(-2);
            ingredientInventory.container[5].addAmount(-2);
            succ();
            CraftableInventory.container[1].addAmount(1);
        }
        else{
            fail();
        }
    }
    public void makePipe(){
        if(ingredientInventory.container[0].amount >= 1 && ingredientInventory.container[2].amount >= 1 && ingredientInventory.container[3].amount >= 1){
            ingredientInventory.container[0].addAmount(-1);
            ingredientInventory.container[2].addAmount(-1);
            ingredientInventory.container[3].addAmount(-1);
            succ();
            CraftableInventory.container[2].addAmount(1);
        }
        else{
            fail();
        }
    }
    public void makeStun(){
        if(ingredientInventory.container[3].amount >= 2 && ingredientInventory.container[5].amount >= 1){
            ingredientInventory.container[3].addAmount(-2);
            ingredientInventory.container[5].addAmount(-1);
            succ();
            CraftableInventory.container[3].addAmount(1);
        }
        else{
            fail();
        }
    }
    public void makeHealth(){
        if(ingredientInventory.container[0].amount >= 2 && ingredientInventory.container[4].amount >= 2){
            ingredientInventory.container[0].addAmount(-2);
            ingredientInventory.container[4].addAmount(-2);
            succ();
            CraftableInventory.container[4].addAmount(1);
        }
        else{
            fail();
        }
    }

    void succ(){
        print("succ");
    }
    void fail(){
        print("fail");
    }
}
