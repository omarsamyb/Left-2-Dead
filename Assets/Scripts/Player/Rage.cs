using System;
using System.Collections;
using UnityEngine;

public class Rage : MonoBehaviour
{
    private float rageResetRef = 3f;
    private float rageDurationRef = 10f;
    public float rageReset;
    private float rageDuration;
    public int ragePoints;
    public bool canActivate;
    public GameObject character;
    private Animation characterAnimation;
    private GunInventory gunInventory;
    [HideInInspector] public bool canBeDamaged;
    private int normalKillPointsRef = 10;
    private int specialKillPointsRef = 50;
    private int normalKillPoints = 10;
    private int specialKillPoints = 50;
    private PlayerVoiceOver pvo;

    public event Action<float> OnRageChange = delegate { };

    void Start()
    {
        rageReset = rageResetRef;
        rageDuration = rageDurationRef;
        ragePoints = 0;
        characterAnimation = character.GetComponent<Animation>();
        gunInventory = GetComponent<GunInventory>();
        canBeDamaged = true;
        pvo = GetComponent<PlayerVoiceOver>();
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
        if (ragePoints == 100 && !canActivate)
        {
            canActivate = true;
            pvo.StartCoroutine(pvo.Rage());
        }
        OnRageChange((float)ragePoints / 100f);
    }

    public IEnumerator ActivateRageMode()
    {
        AudioManager.instance.Stop("PlayerVoice");
        canBeDamaged = false;
        AudioManager.instance.Play("RageModeMusic");
        GameManager.instance.inRageMode = true;
        gunInventory.currentGun.SetActive(false);
        character.SetActive(true);
        characterAnimation.Play("Rage");
        Vector3 origPos = Camera.main.transform.localPosition;
        Vector3 camVector = Camera.main.transform.position - Camera.main.transform.forward;
        camVector.y = 0f;
        camVector += Vector3.up * 1.2f;
        Camera.main.transform.position = camVector;
        Transform origParent = character.transform.parent;
        character.transform.parent = null;
        yield return new WaitForSeconds(0.1f);
        AudioManager.instance.Play("RageModeSFX");
        while (characterAnimation.isPlaying)
            yield return null;
        character.SetActive(false);
        character.transform.SetParent(origParent);
        gunInventory.currentGun.SetActive(true);
        Camera.main.transform.localPosition = origPos;
        canBeDamaged = true;
    }
    public void rageCheats()
    {
        OnRageChange((float)ragePoints / 100f);
    }
}
