using System.Collections;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    private Light lightSource;
    private float lightTimer;

    void Start()
    {
        AudioManager.instance.Play("RainSFX");
        AudioManager.instance.Play("RattleSFX");
        lightSource = GetComponent<Light>();
        lightTimer = Random.Range(10, 15);
    }

    void Update()
    {
        if (lightTimer <= 0)
        {
            lightTimer = Random.Range(15, 20);
            StartCoroutine(ThunderEffect());
        }
        else
            lightTimer -= Time.deltaTime;
    }

    IEnumerator ThunderEffect()
    {
        AudioManager.instance.Play("ThunderSFX");
        lightSource.intensity = 1f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 0.4f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 1f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 0.4f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 1f;
        yield return new WaitForSeconds(0.1f);
        lightSource.intensity = 0;
        yield return new WaitForSeconds(2f);
        lightSource.intensity = 1f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 0f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 1f;
        yield return new WaitForSeconds(0.05f);
        lightSource.intensity = 0f;
    }
}
