using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Craftable Object", menuName = "Iventory/Items/Craftable")]
public class CraftableObject : ItemObject
{
    public void Awake(){
        type = ItemType.Craftable;
    }
}
