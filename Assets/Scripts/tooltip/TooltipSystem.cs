using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{

    private static TooltipSystem current;

    public Tooltip tooltip;

    public void Awake(){
        current = this;
    }

    public static void Show(string content, string header, string behaviour){
        current.tooltip.SetText(content,header,behaviour);
        current.tooltip.gameObject.SetActive(true);
    }

    public static void Reset(){
        current.tooltip.SetText("","","");
        current.tooltip.gameObject.SetActive(true);
    }

    public static void Hide(){
        current.tooltip.gameObject.SetActive(false);
    }
}
