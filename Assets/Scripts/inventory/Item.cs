using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemObject item;
    [HideInInspector] public GameObject gameObj;
    [HideInInspector] public Vector3 itemPos;
    public int amount = 2;
    void Start(){
        gameObj = this.gameObject;
        itemPos = transform.position;
    }
}
