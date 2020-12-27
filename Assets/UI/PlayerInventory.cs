using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject inventory;
    
    void OnTriggerEnter(Collider other)
    {
        print("innn");
        var item = other.GetComponent<Item>();
        if(item){
            inventory.addItem(item.item, 1);
            Destroy(other.gameObject);
        }
    }

    void OnApplicationQuit()
    {
        for(int i = 0; i < inventory.container.Count; i++){
            inventory.container[i].resetAmount();
        }
    }
}
