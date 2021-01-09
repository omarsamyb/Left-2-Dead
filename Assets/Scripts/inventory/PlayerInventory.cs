using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject ingredientInventory;
    public InventoryObject AmmoInventory;
    public GameObject inventoryPanel;

    public bool inventoryPanelToggle;

    void Start(){
        inventoryPanelToggle = false;
    }
    void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<Item>();
        if(obj){ // check if object has the script item
        
            if(obj.item.type == ItemType.Ingredient){ // check if the item is of type ingredient then add it to ingredient inventory
                ingredientInventory.addItem(obj.item, 1);
                Destroy(other.gameObject);
            }

            if(obj.item.type == ItemType.Ammo){ // check if the item is of type Ammo then add it to Ammo inventory
                AmmoInventory.addItem(obj.item, 1);
                Destroy(other.gameObject);
            }
        }
    }

    void OnApplicationQuit() // reseting the inventoryafter exiting the game
    {
        // for(int i = 0; i < ingredientInventory.container.Count; i++){ 
        //     ingredientInventory.container[i].resetAmount();
        // }

        for(int i = 0; i < AmmoInventory.container.Count; i++){ 
            AmmoInventory.container[i].resetAmount();
        }
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanelToggle = !inventoryPanelToggle;
            inventoryPanel.SetActive(inventoryPanelToggle);
        }
    }

    public void SetinventoryPanel(bool val){
        inventoryPanel.SetActive(val);
    }
}
