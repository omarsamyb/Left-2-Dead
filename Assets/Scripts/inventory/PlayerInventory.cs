using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject ingredientInventory;
    public InventoryObject AmmoInventory;
    GameObject player;
    public List<Item> itemsCollection;
    public List<Item> collectableItems;
    float distanceFromPlayer;

    void Start(){
        itemsCollection = new List<Item>();
        player = PlayerController.instance.player;
        distanceFromPlayer = 2;
        StartCoroutine(GetCollectables());
        
    }

    IEnumerator GetCollectables(){
        collectableItems = new List<Item>();
        List<Item> destroyedItems = new List<Item>();
        string itemsToPick ="";
        if(itemsCollection.Count > 0){
            foreach (Item i in itemsCollection){
                if(i.gameObj == null){
                    print(i.item.name + "was destroyed");
                    destroyedItems.Add(i);
                }
                else{
                    if (Vector3.Distance(player.transform.position , i.itemPos) <= distanceFromPlayer){
                        itemsToPick += i.item.name + " / ";
                        collectableItems.Add(i);
                        //todo show in pickUp menu
                    }
                }
            }
        }
        yield return new WaitForSecondsRealtime(0.01f);
        print(itemsToPick + collectableItems.Count);
        if(destroyedItems.Count > 0){
            foreach(Item i in destroyedItems){
                itemsCollection.Remove(i);
            }
        }
        yield return new WaitForSecondsRealtime(1f);

        StartCoroutine(GetCollectables());
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     var obj = other.GetComponent<Item>();
    //     if(obj){ // check if object has the script item
        
    //         if(obj.item.type == ItemType.Ingredient){ // check if the item is of type ingredient then add it to ingredient inventory
    //             ingredientInventory.addItem(obj.item, 1);
    //             Destroy(other.gameObject);
    //         }

    //         if(obj.item.type == ItemType.Ammo){ // check if the item is of type Ammo then add it to Ammo inventory
    //             AmmoInventory.addItem(obj.item, 1);
    //             Destroy(other.gameObject);
    //         }
    //     }
    // }

    // void OnApplicationQuit() // reseting the inventoryafter exiting the game
    // {
    //     // for(int i = 0; i < ingredientInventory.container.Count; i++){ 
    //     //     ingredientInventory.container[i].resetAmount();
    //     // }

    //     for(int i = 0; i < AmmoInventory.container.Count; i++){ 
    //         AmmoInventory.container[i].resetAmount();
    //     }
    // }

}
