using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    public string content;
    public string behaviour;
    public void OnPointerEnter(PointerEventData eventData){
        Invoke("show",0.2f);
        // TooltipSystem.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData){
        CancelInvoke();
        TooltipSystem.Hide();
    }

    public void show(){
        TooltipSystem.Show(content, header, behaviour);
    }
}
