using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsRoll : MonoBehaviour
{
    public Animator louisAnimator;
    public Animator ellieAnimator;
    private Animator creditsAnimator;
    public Image background;
    public GameObject creditsVideo;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.Stop("BackgroundMusic");
        louisAnimator.SetTrigger("dance");
        ellieAnimator.SetTrigger("dance");
        creditsAnimator = GetComponent<Animator>();
        AudioManager.instance.Play("CreditsMusic");
        StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        yield return new WaitForSecondsRealtime(11f);
        Color color = background.color;
        color.a = 0f;
        background.color = color;
        creditsAnimator.enabled = true;
    }
    public void PlayVideo()
    {
        AudioManager.instance.Stop("CreditsMusic");
        creditsVideo.SetActive(true);
        StartCoroutine(LoadMenu());
    }
    IEnumerator LoadMenu()
    {
        yield return new WaitForSecondsRealtime(41f);
        GameManager.instance.level = 0;
        GameManager.instance.LoadNextLevel();
    }
}
