using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    [SerializeField] private GameObject[] cams;
    [SerializeField] private AudioClip quitSFX;
    [SerializeField] private Image fadeImg;
    [HideInInspector] public bool inMainMenu;
    private int switchIndex;
    private Color color;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        inMainMenu = true;
        AudioManager.instance.Play("MenuBackgroundMusic");
    }
    private void OnDisable()
    {
        color = fadeImg.color;
        color.a = 0f;
        fadeImg.color = color;
    }

    void Update()
    {
        if (fadeImg.color.a != 0)
        {
            color = fadeImg.color;
            color.a -= 0.2f * Time.deltaTime;
            fadeImg.color = color;
        }
    }

    public void ToCompanionMenu()
    {
        inMainMenu = false;
        switchIndex = 1;
        cams[0].SetActive(false);
        cams[1].SetActive(true);
    }
    public void SwitchCompanion(bool direction)
    {
        cams[switchIndex].SetActive(false);
        if(direction)
            switchIndex = (switchIndex + 1) % cams.Length;
        else
        {
            switchIndex--;
            if (switchIndex <= 0)
                switchIndex = cams.Length - 1;
        }
        if (switchIndex == 0)
            switchIndex++;

        cams[switchIndex].SetActive(true);
    }
    public void SelectCompanion()
    {
        GameManager.instance.companionId = --switchIndex;
        AudioManager.instance.Stop("MenuBackgroundMusic");
        AudioManager.instance.Stop("RainSFX");
        GameManager.instance.LoadNextLevel();
    }
    public void SelectEffect()
    {
        AudioManager.instance.Play("ClickSFX");
    }
    public void Quit()
    {
        StartCoroutine(QuitRoutine());
    }
    IEnumerator QuitRoutine()
    {
        AudioManager.instance.SetClip("PlayerVoice", quitSFX);
        AudioManager.instance.Play("PlayerVoice");
        yield return new WaitForSeconds(3f);
        Application.Quit();
    }
}
