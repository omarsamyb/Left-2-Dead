using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIparticleSystem : MonoBehaviour
{
    public float speed;
    public float distToTarget = 0.2f;
    protected bool letPlay = false;
    ParticleSystem particleSystem;
    public List<GameObject> targets;
    float step;
    int targetIndex;
    Vector3 startPos;
    void Start(){
        startPos = this.transform.position;
        particleSystem = gameObject.GetComponent<ParticleSystem>();
        targetIndex = 0;
        step = speed * Time.deltaTime;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            letPlay = !letPlay;
        }
        showOrHide();

        moveTo();

        
    }

    public void playParticle(){
        letPlay = true;
        GetComponent<TrailRenderer>().time = 5;
    }
    public void stopParticle(){
        letPlay = false;
        showOrHide();
        this.transform.position = startPos;
        targetIndex = 0;
        GetComponent<TrailRenderer>().time = -1;
    }

    void moveTo(){
        if(letPlay){
            if(targetIndex < targets.Count){
                transform.position = Vector3.MoveTowards(this.transform.position, targets[targetIndex].transform.position, step);
                float distance = Vector3.Distance (this.transform.position, targets[targetIndex].transform.position);
                if(distance <= distToTarget) targetIndex++;
            }
            // else{
            //     letPlay = false;
            //     showOrHide();
            //     this.transform.position = startPos;
            //     targetIndex = 0;
            // }
        }
    }

    void showOrHide(){
        if(letPlay)
        {
            if(!particleSystem.isPlaying)
            {
                particleSystem.Play();
            }
        }else{
            if(particleSystem.isPlaying)
            {
                particleSystem.Stop();
            }
        }
    }

    public void setTargets(List<GameObject> newTargets){
        targets = newTargets;
    }
}