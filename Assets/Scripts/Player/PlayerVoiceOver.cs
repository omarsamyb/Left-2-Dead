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

    private int rageIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Rage()
    {
        yield return new WaitForSeconds(0.2f);
        AudioManager.instance.SetClip("PlayerVoice", rageClips[rageIndex]);
        AudioManager.instance.Play("PlayerVoice");
        rageIndex = (rageIndex + 1) % rageClips.Length;
    }
}
