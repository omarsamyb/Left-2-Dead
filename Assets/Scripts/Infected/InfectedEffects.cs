using UnityEngine;

public class InfectedEffects : MonoBehaviour
{
    private AudioSource source;
    private InfectedState currentState;
    private InfectedController infectedController;

    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private AudioClip[] chasingClips;
    [SerializeField] private AudioClip[] damagedClips;
    [SerializeField] private AudioClip[] deadClips;
    [SerializeField] private AudioClip[] attackClips;
    private int idleIndex;
    private int chasingIndex;
    private int damagedIndex;
    private int deadIndex;
    private int attackIndex;

    void Start()
    {
        infectedController = GetComponent<InfectedController>();
        source = GetComponent<AudioSource>();
        idleIndex = Random.Range(0, idleClips.Length);
        chasingIndex = Random.Range(0, chasingClips.Length);
        damagedIndex = Random.Range(0, damagedClips.Length);
        deadIndex = Random.Range(0, deadClips.Length);
        attackIndex = Random.Range(0, attackClips.Length);
    }

    void Update()
    {
        currentState = infectedController.state;
        Idle();
        Chasing();
    }

    private void Idle()
    {
        if (!source.isPlaying)
        {
            if (currentState == InfectedState.patrol || currentState == InfectedState.idle || currentState == InfectedState.distraction || infectedController.animator.GetCurrentAnimatorStateInfo(0).IsName("Cooldown"))
            {
                source.clip = idleClips[idleIndex];
                source.Play();
                idleIndex = (idleIndex + 1) % idleClips.Length;
            }
        }
    }
    private void Chasing()
    {
        if (!source.isPlaying)
        {
            if (currentState == InfectedState.chase)
            {
                chasingIndex = Random.Range(0, chasingClips.Length);
                source.clip = chasingClips[chasingIndex];
                source.Play();
                chasingIndex = (chasingIndex + 1) % chasingClips.Length;
            }
        }
    }
    public void Attack(int index)
    {
        if (index == -1)
        {
            source.clip = attackClips[attackIndex];
            source.Play();
            attackIndex = (attackIndex + 1) % attackClips.Length;
        }
        else
        {
            source.clip = attackClips[index];
            source.Play();
        }
    }
    public void Damaged()
    {
        if (!source.isPlaying || currentState != InfectedState.attack || infectedController.animator.GetCurrentAnimatorStateInfo(0).IsName("Cooldown"))
        {
            source.clip = damagedClips[damagedIndex];
            source.Play();
            damagedIndex = (damagedIndex + 1) % damagedClips.Length;
        }
    }
    public void Dead()
    {
        source.clip = deadClips[deadIndex];
        source.Play();
    }
}
