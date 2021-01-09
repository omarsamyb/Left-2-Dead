using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionPanel : MonoBehaviour
{   
    public GameObject zoeyImage;
    public GameObject louisImage;
    public GameObject ellieImage;

    public GameObject bulletCount;
    public GameObject clipCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        setAmmoCount();
    }

    void setAmmoCount(){
        string bullets = "";
        // try
        // {
        //     bullets = CompanionController.bulletsIHave;
        //     print(bullets);
        // }
        // catch (System.Exception ex)
        // {
        //     return;
        // }
    }
}
