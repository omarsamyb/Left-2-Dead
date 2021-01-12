using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light flashLight;
    private static bool isOn = true;
    void Start()
    {
        flashLight = GetComponent<Light>();
    }

    void Update()
    {
        flashLight.enabled = isOn;
        if (Input.GetKeyDown(KeyCode.V))
        {
            AudioManager.instance.Play("ToggleFlashlightSFX");
            isOn = !isOn;
            flashLight.enabled = isOn;
        }
    }
}
