﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsControl : MonoBehaviour
{
    private int fps = 0;
    private GameObject parent;
    private bool on = true;
    // Start is called before the first frame update
    void Start()
    {
        parent = this.gameObject;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (on)
        {
            fps++;
            
            
            if (fps > 5)
            {
                fps = 0;
                light();
            }
        }
        
    }
    private void light()
    {
        int a = new System.Random().Next(0, 10);
        if (a < 3)
        {
            Debug.Log(a);
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;
                if (child != null)
                    child.SetActive(!child.activeSelf);
            }
        }
    }
    public void lightsOff()
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(false);
        }
    }
    public void lightsOn()
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(true);
        }
    }
}
