using UnityEngine;

public class PoseAnimator : MonoBehaviour
{
    private Animator poseAnimator;
    private float poseTimeRef = 10f;
    private float poseTime;

    void Start()
    {
        poseAnimator = GetComponent<Animator>();
        poseTime = 10f;
    }

    void Update()
    {
        if (poseAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Pose"))
        {
            poseTime -= Time.deltaTime;
            if (poseTime <= 0f)
            {
                poseTime = poseTimeRef;
                poseAnimator.SetTrigger("switchPose");
            }
        }
    }
}
