using System.Collections;
using UnityEngine;

public class PlayerVoiceOver : MonoBehaviour
{
    public AudioClip[] rageClips;
    public AudioClip[] detectionClips;
    public AudioClip[] spottedClips;
    public AudioClip[] fightFinishedClips;
    public AudioClip[] equipmentClips;
    public AudioClip[] bileClips;
    public AudioClip[] unpinnedClips;
    public AudioClip[] pinnedClips;
    private int rageIndex;
    private int pinnedIndex;
    private int bileIndex;
    private int detectionIndex;
    private int spottedIndex;
    private int fightFinishedIndex;

    RaycastHit hitinfo;
    private LayerMask enemyLayer;
    private LayerMask detectionLayer;
    private float detectionRange = 40f;
    private float detectionRateRef = 0.5f;
    private float detectionTime;
    private bool isDetecting;
    private bool detected;
    private bool spotted;
    [HideInInspector] public bool inFight;
    private float fightTimerRef = 0.5f;
    private float fightTimer;

    private CompanionVoiceOver cvo;

    void Start()
    {
        detectionTime = detectionRateRef;
        fightTimer = fightTimerRef;
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        detectionLayer = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Weapon"));
        cvo = CompanionController.instance.transform.GetComponent<CompanionVoiceOver>();
    }

    void Update()
    {
        if (detectionTime > 0f)
            detectionTime -= Time.deltaTime;
        else if(detectionTime <= 0f && !isDetecting)
        {
            isDetecting = true;
            StartCoroutine(Detection());
        }

        if(inFight && !spotted)
        {
            spotted = true;
            StartCoroutine(Spotted());
        }

        if(inFight && !detected)
            fightTimer -= Time.deltaTime;
        if(fightTimer <= 0f)
        {
            fightTimer = fightTimerRef;
            spotted = false;
            inFight = false;
            FightFinished();
        }
    }

    public IEnumerator Rage()
    {
        yield return new WaitForSeconds(0.3f);
        AudioManager.instance.SetClip("PlayerVoice", rageClips[rageIndex]);
        AudioManager.instance.Play("PlayerVoice");
        rageIndex = (rageIndex + 1) % rageClips.Length;
    }

    public IEnumerator Unpinned(int index)
    {
        yield return new WaitForSeconds(0.3f);
        AudioManager.instance.SetClip("PlayerVoice", unpinnedClips[index]);
        AudioManager.instance.Play("PlayerVoice");
    }

    public IEnumerator Pinned()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.instance.SetClip("PlayerVoice", pinnedClips[pinnedIndex]);
        AudioManager.instance.Play("PlayerVoice");
        pinnedIndex = (pinnedIndex + 1) % pinnedClips.Length;
    }
    public IEnumerator Bile()
    {
        yield return new WaitForSeconds(0.3f);
        AudioManager.instance.SetClip("PlayerVoice", bileClips[bileIndex]);
        AudioManager.instance.Play("PlayerVoice");
        bileIndex = (bileIndex + 1) % bileClips.Length;
    }
    IEnumerator Detection()
    {
        Collider[] hits = Physics.OverlapBox(transform.position + transform.forward * detectionRange/2f + transform.up, new Vector3(detectionRange/2f, 1f, detectionRange/2f), Quaternion.identity, enemyLayer);
        
        bool currentDetection = false;
        foreach (Collider collider in hits)
        {
            if (Physics.Raycast(transform.position, collider.transform.position - transform.position, out hitinfo, detectionRange, detectionLayer) && hitinfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                currentDetection = true;
                if (!AudioManager.instance.isPlaying("PlayerVoice") && !detected)
                {
                    detected = true;
                    if (Random.Range(0, 10) == 5)
                        cvo.Detection();
                    else
                    {
                        AudioManager.instance.SetClip("PlayerVoice", detectionClips[detectionIndex]);
                        AudioManager.instance.Play("PlayerVoice");
                        detectionIndex = (detectionIndex + 1) % detectionClips.Length;
                    }
                }
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (!currentDetection)
            detected = false;

        detectionTime = detectionRateRef;
        isDetecting = false;
    }
    IEnumerator Spotted()
    {
        yield return new WaitForSeconds(0.3f);
        if (!AudioManager.instance.isPlaying("PlayerVoice"))
        {
            AudioManager.instance.SetClip("PlayerVoice", spottedClips[spottedIndex]);
            AudioManager.instance.Play("PlayerVoice");
            spottedIndex = (spottedIndex + 1) % spottedClips.Length;
        }
    }
    private void FightFinished()
    {
        AudioManager.instance.SetClip("PlayerVoice", fightFinishedClips[fightFinishedIndex]);
        AudioManager.instance.Play("PlayerVoice");
        fightFinishedIndex = (fightFinishedIndex + 1) % fightFinishedClips.Length;
    }
}
