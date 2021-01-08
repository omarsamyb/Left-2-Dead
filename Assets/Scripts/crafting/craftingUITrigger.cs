using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class craftingUITrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject img;
    public List<GameObject> flameParticles;
    public List<GameObject> targets;
    public void OnPointerEnter(PointerEventData eventData){
        for(int i=0;i<flameParticles.Count;i++){
            UIparticleSystem uiPS = flameParticles[i].GetComponent<UIparticleSystem>();
            uiPS.setTargets(targets);
            uiPS.playParticle();
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        for(int i=0;i<flameParticles.Count;i++){
            UIparticleSystem uiPS = flameParticles[i].GetComponent<UIparticleSystem>();
            uiPS.stopParticle();
        }
    }

}

