using System;
using System.Collections;
using UnityEngine;

public class Rage : MonoBehaviour
{
    private float rageResetRef = 3f;
    private float rageDurationRef = 10f;
    private float rageReset;
    private float rageDuration;
    private int ragePoints;
    private bool canActivate;
    private TextMesh HUD_rage;
    public GameObject character;
    private Animation characterAnimation;
    private GunInventory gunInventory;
    [HideInInspector] public bool canBeDamaged;
    private int normalKillPointsRef = 10;
    private int specialKillPointsRef = 50;
    private int normalKillPoints = 10;
    private int specialKillPoints = 50;

    public event Action<float> OnRageChange = delegate { };

    void Start()
    {
        rageReset = rageResetRef;
        rageDuration = rageDurationRef;
        ragePoints = 0;
        characterAnimation = character.GetComponent<Animation>();
        gunInventory = GetComponent<GunInventory>();
        canBeDamaged = true;
    }

    void Update()
    {
        if (GameManager.instance.companionId == 0)
        {
            if (CompanionController.instance.canApplyAbility)
            {
                normalKillPoints = normalKillPointsRef * 2;
                specialKillPoints = specialKillPointsRef * 2;
            }
            else
            {
                normalKillPoints = normalKillPointsRef;
                specialKillPoints = specialKillPointsRef;
            }
        }

        if (rageReset >= 0)
            rageReset -= Time.deltaTime;
        if (rageReset <= 0 && !canActivate && ragePoints != 0)
        {
            ragePoints = 0;
            OnRageChange(0f);
        }

        if (GameManager.instance.inRageMode)
        {
            rageDuration -= Time.deltaTime;
            if (rageDuration <= 0f)
            {
                GameManager.instance.inRageMode = false;
                ragePoints = 0;
                OnRageChange(0f);
                canActivate = false;
                rageDuration = rageDurationRef;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && canActivate && !GameManager.instance.inRageMode && PlayerController.instance.CanDoIt())
            StartCoroutine(ActivateRageMode());
    }

    public void UpdateRage(string tag)
    {
        rageReset = rageResetRef;
        if (tag[0] == 'S')
            ragePoints = Mathf.Clamp(ragePoints + specialKillPoints, 0, 100);
        else
            ragePoints = Mathf.Clamp(ragePoints + normalKillPoints, 0, 100);
        if (ragePoints == 100)
            canActivate = true;
        OnRageChange((float)ragePoints / 100f);
    }

    IEnumerator ActivateRageMode()
    {
        canBeDamaged = false;
        AudioManager.instance.Play("RageModeMusic");
        GameManager.instance.inRageMode = true;
        gunInventory.currentGun.SetActive(false);
        character.SetActive(true);
        characterAnimation.Play("Rage");
        Camera.main.transform.position -= Camera.main.transform.forward + Camera.main.transform.up * 0.5f;
        Transform origParent = character.transform.parent;
        character.transform.parent = null;
        yield return new WaitForSeconds(0.1f);
        AudioManager.instance.Play("RageModeSFX");
        while (characterAnimation.isPlaying)
            yield return null;
        character.SetActive(false);
        character.transform.SetParent(origParent);
        gunInventory.currentGun.SetActive(true);
        Camera.main.transform.position += Camera.main.transform.forward + Camera.main.transform.up * 0.5f;
        canBeDamaged = true;
    }

    void OnGUI()
    {
        if (!HUD_rage)
        {
            try
            {
                HUD_rage = GameObject.Find("HUD_rage").GetComponent<TextMesh>();
            }
            catch (System.Exception ex)
            {
                print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
            }
        }
        if (HUD_rage)
            HUD_rage.text = ragePoints.ToString() + " - " + rageReset.ToString() + " - " + rageDuration.ToString();
    }
}
