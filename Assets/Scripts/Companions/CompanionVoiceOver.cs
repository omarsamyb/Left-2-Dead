using System.Collections;
using UnityEngine;

public class CompanionVoiceOver : MonoBehaviour
{
    public AudioSource voiceOverSource;
    public AudioClip[] detectionClips;
    public AudioClip[] friendlyFireClips;
    public AudioClip[] healUpClips;
    public AudioClip[] orderClips;
    public AudioClip[] genericClips;
    public AudioClip[] killClips;
    private int detectionIndex;
    private int friendlyFireIndex;
    private int healUpIndex;
    private int orderIndex;
    private int killIndex;

    private float healUpTimerRef = 10f;
    private float healUpTimer;
    private float orderTimerRef = 1f;
    private float orderTimer;
    private float killTimerRef = 3f;
    private float killTimer;

    void Start()
    {
        healUpTimer = healUpTimerRef;
        orderTimer = orderTimerRef;
        killTimer = killTimerRef;
        detectionIndex = Random.Range(0, detectionClips.Length);
        friendlyFireIndex = Random.Range(0, friendlyFireClips.Length);
        healUpIndex = Random.Range(0, healUpClips.Length);
        orderIndex = Random.Range(0, orderClips.Length);
        killIndex = Random.Range(0, killClips.Length);
    }

    void Update()
    {
        if (healUpTimer > 0)
            healUpTimer -= Time.deltaTime;
        if (orderTimer > 0)
            orderTimer -= Time.deltaTime;
        if (killTimer > 0)
            killTimer -= Time.deltaTime;
    }
    
    public void Detection()
    {
        if (!voiceOverSource.isPlaying)
        {
            voiceOverSource.clip = detectionClips[detectionIndex];
            voiceOverSource.Play();
            detectionIndex = (detectionIndex + 1) % detectionClips.Length;
        }
    }
    public void FriendlyFire()
    {
        if (!voiceOverSource.isPlaying)
        {
            voiceOverSource.clip = friendlyFireClips[friendlyFireIndex];
            voiceOverSource.Play();
            friendlyFireIndex = (friendlyFireIndex + 1) % friendlyFireClips.Length;
        }
    }
    public void HealUp()
    {
        if (healUpTimer <= 0 && !voiceOverSource.isPlaying)
        {
            healUpTimer = healUpTimerRef;
            voiceOverSource.clip = healUpClips[healUpIndex];
            voiceOverSource.Play();
            healUpIndex = (healUpIndex + 1) % healUpClips.Length;
        }
    }
    public void PlayerDeath()
    {
        voiceOverSource.clip = genericClips[1];
        voiceOverSource.Play();
    }
    public IEnumerator Order()
    {
        if(orderTimer <= 0)
        {
            yield return new WaitForSeconds(0.4f);
            orderTimer = orderTimerRef;
            if (!voiceOverSource.isPlaying)
            {
                voiceOverSource.clip = orderClips[orderIndex];
                voiceOverSource.Play();
                orderIndex = (orderIndex + 1) % orderClips.Length;
            }
        }
        orderTimer = orderTimerRef;
    }
    public IEnumerator Kill()
    {
        if (killTimer <= 0)
        {
            yield return new WaitForSeconds(0.3f);
            killTimer = killTimerRef;
            voiceOverSource.clip = killClips[killIndex];
            voiceOverSource.Play();
            killIndex = (killIndex + 1) % killClips.Length;
        }
    }
}
