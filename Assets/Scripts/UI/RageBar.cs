using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RageBar : MonoBehaviour
{
    private float updateTimeSeconds;
    private float updateTime;
    private bool coroutineRunning;
    private bool skipUpdate;
    public GameObject overlay;

    private void Start()
    {
        PlayerController.instance.player.GetComponent<Rage>().OnRageChange += HandleRageChange;
        updateTimeSeconds = 0.5f;
    }
    private void Update()
    {
        if (GameManager.instance.inRageMode && !overlay.activeSelf)
            overlay.SetActive(true);
        if (!GameManager.instance.inRageMode && overlay.activeSelf)
            overlay.SetActive(false);
    }
    private void HandleRageChange(float percentage)
    {
        if (coroutineRunning)
            skipUpdate = true;
        StartCoroutine(ModifyBar(percentage));
    }

    IEnumerator ModifyBar(float percentage)
    {
        coroutineRunning = true;
        yield return new WaitForEndOfFrame();
        Image bar = GetComponent<Image>();
        float currentPercentage = bar.fillAmount;
        updateTime = 0f;

        while(updateTime < updateTimeSeconds)
        {
            if (skipUpdate)
                break;
            updateTime += Time.deltaTime;
            bar.fillAmount = Mathf.Lerp(currentPercentage, percentage, updateTime / updateTimeSeconds);
            yield return null;
        }
        bar.fillAmount = percentage;
        coroutineRunning = false;
        skipUpdate = false;
    }
}
