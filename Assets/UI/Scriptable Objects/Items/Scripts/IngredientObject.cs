using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient Object", menuName = "Iventory/Items/Ingredient")]
public class IngredientObject : ItemObject
{
    public void Awake(){
        type = ItemType.Ingredient;
    }
}
