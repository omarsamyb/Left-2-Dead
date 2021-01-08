using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Display : MonoBehaviour
{
    Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
    public InventoryObject inventory;
    // Start is called before the first frame update
    void Start()
    {
        createDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDisplay();
    }

    public void createDisplay(){
        
        for(int i = 0; i<inventory.container.Count; i++){
            var obj = Instantiate(inventory.container[i].item.prefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].amount.ToString();
            itemsDisplayed.Add(inventory.container[i], obj);
        }
    }

    public void UpdateDisplay()
    {
        for(int i = 0; i<inventory.container.Count; i++){
            if(itemsDisplayed.ContainsKey(inventory.container[i])){
                itemsDisplayed[inventory.container[i]].GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].amount.ToString();
            }
            else{
                var obj = Instantiate(inventory.container[i].item.prefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = inventory.container[i].amount.ToString();
                itemsDisplayed.Add(inventory.container[i], obj);
            }
        }
    }
}
