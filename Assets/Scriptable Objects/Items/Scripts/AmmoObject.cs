using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Ammo Object", menuName = "Iventory/Items/Ammo")]
public class AmmoObject : ItemObject
{
    public void Awake(){
        type = ItemType.Ammo;
    }
}
