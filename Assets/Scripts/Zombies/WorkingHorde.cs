using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class WorkingHorde : MonoBehaviour
{
    public GameObject normal;
    public GameObject boomer;
    public GameObject charger;
    public GameObject hunter;
    public GameObject spitter;
    public GameObject tank;

    public enum hordeType { normal, oneBoomer, oneCharger, oneHunter, oneSpitter, oneTank, oneRandomSpecial};
    public hordeType horde;

    // Start is called before the first frame update
    void Start()
    {
        if(horde == hordeType.normal)
            createZombie(normal, transform.position);
        else if(horde == hordeType.oneBoomer)
            createZombie(boomer, transform.position);
        else if(horde == hordeType.oneCharger)
            createZombie(charger, transform.position);
        else if(horde == hordeType.oneHunter)
            createZombie(hunter, transform.position);
        else if(horde == hordeType.oneSpitter)
            createZombie(spitter, transform.position);
        else if(horde == hordeType.oneTank)
            createZombie(tank, transform.position);
        else if(horde == hordeType.oneRandomSpecial)
            createZombie(pickRandomZombie(), transform.position);
        for(int i=-2;i<3;i++)
            for(int j=-1;j<3;j++)
                if(i!=0 || j!=0)
                    createZombie(normal, new Vector3(i, 0, j*0.5f) + transform.position);
        Destroy(gameObject);
    }
    void createZombie(GameObject zombie, Vector3 location)
    {
        NavMeshHit myNavHit;
        if(NavMesh.SamplePosition(location, out myNavHit, 6f , -1))
        {
            Instantiate(zombie, myNavHit.position, transform.rotation);
        }
    }
    GameObject pickRandomZombie()
    {
        float randNumber = Random.Range(0.0f, 5.0f);
        if(randNumber<1)
            return boomer;
        else if(randNumber<2)
            return charger;
        else if(randNumber<3)
            return hunter;
        else if(randNumber<4)
            return spitter;
        else if(randNumber<5)
            return tank;
        return null;
    }
}
