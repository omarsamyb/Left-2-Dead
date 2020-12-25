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

    // void OnApplicationQuit()
    // {
    //     inventory.container.Clear(); 
    // }
}
