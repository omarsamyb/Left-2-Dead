using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class craftingUITrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject img;
    
    public void OnPointerEnter(PointerEventData eventData){
        img.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData){
        img.SetActive(false);
    }

}

