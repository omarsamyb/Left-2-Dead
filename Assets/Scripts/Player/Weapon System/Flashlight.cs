using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light flashLight;
    void Start()
    {
        flashLight = GetComponent<Light>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            AudioManager.instance.Play("ToggleFlashlightSFX");
            flashLight.enabled = !flashLight.enabled;
        }
    }
}
