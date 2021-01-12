using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject ingredientInventory;
    public InventoryObject ammoInventory;
    public InventoryObject craftableInventory;
    GameObject player;
    public List<Item> itemsCollection;
    public int[] collectableItemsCount;
    float distanceFromPlayer;
    List<Item> collectableItems;
    List<Item> destroyedItems;
    void Start(){
        itemsCollection = new List<Item>();
        player = PlayerController.instance.player;
        distanceFromPlayer = 2;
        StartCoroutine(GetCollectables());
        
    }

    IEnumerator GetCollectables(){
        collectableItemsCount = new int[9];
        collectableItems = new List<Item>();
        destroyedItems = new List<Item>();
        string itemsToPick ="";
        if(itemsCollection.Count > 0){
            foreach (Item i in itemsCollection){
                if(i.gameObj == null){
                    destroyedItems.Add(i);
                }
                else{
                    if (Vector3.Distance(player.transform.position , i.itemPos) <= distanceFromPlayer){
                        itemsToPick += i.item.name + " / ";
                        collectableItems.Add(i);
                        if(i.item.name == "Heavy Ammo"){
                            collectableItemsCount[0] += i.amount;
                        }
                        else if(i.item.name == "Light Ammo"){
                            collectableItemsCount[1] += i.amount;
                        }
                        else if(i.item.name == "Shotgun shells"){
                            collectableItemsCount[2] += i.amount;
                        }
                        else if(i.item.name == "Alcohol"){
                            collectableItemsCount[3] += i.amount;
                        }
                        else if(i.item.name == "Canister"){
                            collectableItemsCount[4] += i.amount;
                        }
                        else if(i.item.name == "Sugar"){
                            collectableItemsCount[5] += i.amount;
                        }
                        else if(i.item.name == "GunPowder"){
                            collectableItemsCount[6] += i.amount;
                        }
                        else if(i.item.name == "Rag"){
                            collectableItemsCount[7] += i.amount;
                        }
                        else if(i.item.name == "Health pack"){
                            collectableItemsCount[8] += i.amount;
                        }
                    }
                }
            }
        }
        yield return new WaitForSecondsRealtime(0.01f);
        if(destroyedItems.Count > 0){
            foreach(Item i in destroyedItems){
                itemsCollection.Remove(i);
            }
        }
        yield return new WaitForSecondsRealtime(0.03f);
        StartCoroutine(GetCollectables());
    }


    public void pickUp(string itemName){
        List<Item> remove = new List<Item>();
        
        foreach (Item i in collectableItems){
            if(i.item.name == itemName){
                Destroy(i.gameObj);
                remove.Add(i);
            }
        }

        foreach(Item i in destroyedItems){
            collectableItems.Remove(i);
        }

        if(itemName == "Heavy Ammo"){
            ammoInventory.addItem(itemName, collectableItemsCount[0]);
        }
        else if(itemName == "Light Ammo"){
            ammoInventory.addItem(itemName, collectableItemsCount[1]);
        }
        else if(itemName == "Shotgun shells"){
            ammoInventory.addItem(itemName, collectableItemsCount[2]);
        }
        else if(itemName == "Alcohol"){
            ingredientInventory.addItem(itemName, collectableItemsCount[3]);
        }
        else if(itemName == "Canister"){
            ingredientInventory.addItem(itemName, collectableItemsCount[4]);
        }
        else if(itemName == "Sugar"){
            ingredientInventory.addItem(itemName, collectableItemsCount[5]);
        }
        else if(itemName == "GunPowder"){
            ingredientInventory.addItem(itemName, collectableItemsCount[6]);
        }
        else if(itemName == "Rag"){
            ingredientInventory.addItem(itemName, collectableItemsCount[7]);
        }
        else if(itemName == "Health pack"){
            craftableInventory.addItem(itemName, collectableItemsCount[8]);
        }
    }

}
