using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemObject item;
    [HideInInspector] public GameObject gameObj;
    [HideInInspector] public Vector3 itemPos;
    PlayerInventory playerInventory; 
    public int amount = 2;
    void Start(){
        gameObj = this.gameObject;
        itemPos = transform.position;
        StartCoroutine(init());
    }

    IEnumerator init(){
        yield return new WaitForSecondsRealtime(0.5f);
        playerInventory = PlayerController.instance.player.GetComponent<PlayerInventory>();
        playerInventory.itemsCollection.Add(this);
    }
}
