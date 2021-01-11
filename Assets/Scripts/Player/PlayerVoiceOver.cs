using System.Collections;
using System.Collections.Generic;
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

    RaycastHit hitinfo;
    private LayerMask enemyLayer;
    private LayerMask detectionLayer;
    private float detectionRange = 40f;
    private float detectionRateRef = 1f;
    private float detectionTime;
    private bool isDetecting;
    private bool detected;

    private bool inFight;

    void Start()
    {
        detectionTime = detectionRateRef;
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        detectionLayer = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Weapon"));
    }

    void Update()
    {
        if (detectionTime > 0f)
            detectionTime -= Time.deltaTime;
        else if(detectionTime <= 0f && !isDetecting && !inFight)
        {
            isDetecting = true;
            StartCoroutine(Detection());
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
                    AudioManager.instance.SetClip("PlayerVoice", detectionClips[detectionIndex]);
                    AudioManager.instance.Play("PlayerVoice");
                    detectionIndex = (detectionIndex + 1) % detectionClips.Length;
                }
                break;
            }
            if (inFight)
                break;
            yield return new WaitForSeconds(0.1f);
        }
        if (!currentDetection)
            detected = false;

        detectionTime = detectionRateRef;
        isDetecting = false;
    }
}
