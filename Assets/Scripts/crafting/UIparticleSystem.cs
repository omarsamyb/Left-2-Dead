using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIparticleSystem : MonoBehaviour
{
    public float speed;
    public float distToTarget = 0.2f;
    protected bool letPlay = false;
    protected bool showFlame = false;
    ParticleSystem particleSystem;
    public List<GameObject> targets;
    float step;
    int targetIndex;
    Vector3 startPos;

    public int myInd;
    public InventoryObject ingredientInventory;
    // alcohol  bile  canister  gunpowder  rag  sugar
    //    0      1       2         3        4     5


    void Start(){
        startPos = this.transform.position;
        particleSystem = gameObject.GetComponent<ParticleSystem>();
        targetIndex = 0;
        step = speed * Time.unscaledDeltaTime;
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
        showFlame = haveEnough();
        letPlay = true;
        GetComponent<TrailRenderer>().time = 5;
    }
    public void stopParticle(){
        letPlay = false;
        showFlame = false;
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
            else{
                letPlay = false;
                showFlame = false;
                showOrHide();
            }
        }
    }

    void showOrHide(){
        if(showFlame)
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

    bool haveEnough(){
        string myName = this.name;
        if(myName == "alcohol FS"){
            if(targets[0].name == "MolotovTarget"){
                return ingredientInventory.container[0].amount >= 2 ;
            }
            else if(targets[0].name == "HealthTarget"){
                return ingredientInventory.container[0].amount >= 2 ;
            }
            else if(targets[0].name == "PipeTarget"){
                return ingredientInventory.container[0].amount >= 1 ;
            }
        }
        else if(myName == "bile FS"){
            if(targets[0].name == "BileTarget"){
                return ingredientInventory.container[1].amount >= 1 ;
            }
        }
        else if(myName == "canister FS"){
            if(targets[0].name == "BileTarget"){
                return ingredientInventory.container[2].amount >= 1 ;
            }
            else if(targets[0].name == "PipeTarget"){
                return ingredientInventory.container[2].amount >= 1 ;
            }
        }
        else if(myName == "gunpowder FS"){
            if(targets[0].name == "BileTarget"){
                return ingredientInventory.container[3].amount >= 1 ;
            }
            else if(targets[0].name == "PipeTarget"){
                return ingredientInventory.container[3].amount >= 1 ;
            }
            else if(targets[0].name == "StunTarget"){
                return ingredientInventory.container[3].amount >= 2 ;
            }
        }
        else if(myName == "rag FS"){
            if(targets[0].name == "MolotovTarget"){
                return ingredientInventory.container[4].amount >= 2 ;
            }
            else if(targets[0].name == "HealthTarget"){
                return ingredientInventory.container[4].amount >= 2 ;
            }
        }
        else if(myName == "sugar FS"){
            if(targets[0].name == "StunTarget"){
                return ingredientInventory.container[4].amount >= 1 ;
            }
        }
        return false;
    }
}