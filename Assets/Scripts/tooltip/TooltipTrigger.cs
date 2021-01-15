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
        StartCoroutine(show());
        // TooltipSystem.Show(content, header, behaviour);
    }

    public void OnPointerExit(PointerEventData eventData){
        CancelInvoke();
        TooltipSystem.Hide();
    }

    IEnumerator show(){
        yield return new WaitForSecondsRealtime(0.05f);
        TooltipSystem.Show(content, header, behaviour);
    }
}
