using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory ", menuName = "Iventory/Inventory")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> container = new List<InventorySlot>();

    public void addItem(ItemObject obj, int num){
        bool hasItem = false;
        for(int i =0; i < container.Count; i++){
            if(container[i].item == obj)
            {
                container[i].addAmount(num);
                hasItem = true;
                break;
            }
        }
        if(!hasItem){
            container.Add(new InventorySlot(obj, num));
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;

    public InventorySlot(ItemObject obj, int num){
        item = obj;
        amount = num;
    }

    public void addAmount(int val){
        amount += val;
    }
}