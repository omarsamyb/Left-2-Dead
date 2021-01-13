using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class pickUpPanel : MonoBehaviour
{
    public Button[] buttons;
    PlayerInventory playerInventory; 
    
    void Start(){
        StartCoroutine(init());
    }

    IEnumerator init(){
        yield return new WaitForSecondsRealtime(0.5f);
        playerInventory = PlayerController.instance.player.GetComponent<PlayerInventory>();
    }
    
    void Update()
    {
        setButtons();
    }

    void setButtons(){
        int[] itemCount = PlayerController.instance.player.GetComponent<PlayerInventory>().collectableItemsCount;
        string s = "";
        for(int i=0;i<itemCount.Length;i++){
            s += itemCount[i]+" "; 
        }
        for(int i = 0; i < buttons.Length; i++){
            if(itemCount[i] > 0){
                buttons[i].GetComponentsInChildren<TextMeshProUGUI>()[0].SetText(itemCount[i]+"");
                if(!buttons[i].gameObject.activeSelf){
                    buttons[i].gameObject.SetActive(true);
                    // print(s);
                }
            }
            else{
                if(buttons[i].gameObject.activeSelf)
                    buttons[i].gameObject.SetActive(false);
            }
        }
    }
}
