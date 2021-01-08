using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Ingredient,
    Craftable,
    Ammo,
    Weapon
}
public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType type;

}
