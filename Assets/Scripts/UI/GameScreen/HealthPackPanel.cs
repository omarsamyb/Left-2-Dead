using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthPackPanel : MonoBehaviour
{
    public GameObject HealthPackImage;

    public GameObject packCount;
    public InventoryObject CraftableInventory;

    bool packSet = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        setPack();
    }

    void setPack(){
        int numOfPacks = CraftableInventory.container[4].amount;
        if(numOfPacks > 0){
            if(!packSet){
                HealthPackImage.SetActive(true);
                packSet = true;
            }
            if(int.Parse(packCount.GetComponent<TextMeshProUGUI>().text) != numOfPacks){
                packCount.GetComponent<TextMeshProUGUI>().SetText(numOfPacks+"");
            }
        }
    }
}
