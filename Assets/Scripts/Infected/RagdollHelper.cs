using UnityEngine;
using System.Collections.Generic;

/*
A helper component that enables blending from Mecanim animation to ragdolling and back. 

To use, do the following:

Add "GetUpFromBelly" and "GetUpFromBack" bool inputs to the Animator controller
and corresponding transitions from any state to the get up animations. When the ragdoll mode
is turned on, Mecanim stops where it was and it needs to transition to the get up state immediately
when it is resumed. Therefore, make sure that the blend times of the transitions to the get up animations are set to zero.

TODO:

Make matching the ragdolled and animated root rotation and position more elegant. Now the transition works only if the ragdoll has stopped, as
the blending code will first wait for mecanim to start playing the get up animation to get the animated hip position and rotation. 
Unfortunately Mecanim doesn't (presently) allow one to force an immediate transition in the same frame. 
Perhaps there could be an editor script that precomputes the needed information.

*/

public class RagdollHelper : MonoBehaviour
{
    //Declare a class that will hold useful information for each body part
    public class BodyPart
    {
        public Transform transform;
        public Vector3 storedPosition;
        public Quaternion storedRotation;
    }

    //How long do we blend when transitioning from ragdolled to animated
    public float ragdollToMecanimBlendTime = 0.5f;
    float mecanimToGetUpTransitionTime = 0.05f;
    //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
    float ragdollingEndTime = -100;
    //Additional vectores for storing the pose the ragdoll ended up in.
    Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;
    //Declare a list of body parts, initialized in Start()
    List<BodyPart> bodyParts = new List<BodyPart>();
    //Declare an Animator member variable, initialized in Start to point to this gameobject's Animator component.
    Animator anim;
    Transform skeleton;
    Component[] rigidBodies;

    public enum RagdollState
    {
        animated,    //Mecanim is fully in control
        ragdolled,   //Mecanim turned off, physics controls the ragdoll
        blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
    }
    public RagdollState state = RagdollState.animated;

    void Start()
    {
        skeleton = GetComponent<InfectedController>().skeleton;
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        Component[] transforms = skeleton.GetComponents<Transform>();
        foreach (Transform t in transforms)
        {
            BodyPart bodyPart = new BodyPart
            {
                transform = t
            };
            bodyParts.Add(bodyPart);
        }
        anim = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (state == RagdollState.blendToAnim)
        {
            if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
            {
                //If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
                //character to the best match with the ragdoll
                Vector3 animatedToRagdolled = ragdolledHipPosition - anim.GetBoneTransform(HumanBodyBones.Hips).position;
                Vector3 newRootPosition = transform.position + animatedToRagdolled;

                //Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
                RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition, Vector3.down));
                newRootPosition.y = 0;
                foreach (RaycastHit hit in hits)
                {
                    if (!hit.transform.IsChildOf(transform))
                    {
                        newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                    }
                }
                transform.position = newRootPosition;

                //Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
                Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
                ragdolledDirection.y = 0;

                Vector3 meanFeetPosition = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + anim.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = anim.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0;

                //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                //hence setting the y components of the vectors to zero. 
                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
            }
            //compute the ragdoll blend amount in the range 0...1
            float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
            ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

            //In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
            //To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
            //and slerp all the rotations towards the ones stored when ending the ragdolling
            foreach (BodyPart b in bodyParts)
            {
                if (b.transform != transform)
                { //this if is to prevent us from modifying the root of the character, only the actual body parts
                  //position is only interpolated for the hips
                    if (b.transform == anim.GetBoneTransform(HumanBodyBones.Hips))
                        b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
                    //rotation is interpolated for all body parts
                    b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
                }
            }

            //if the ragdoll blend amount has decreased to zero, move to animated state
            if (ragdollBlendAmount == 0)
            {
                state = RagdollState.animated;
                return;
            }
        }
    }

    public void ActivateRagdoll()
    {
        //Transition from animated to ragdolled
        //setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
        anim.enabled = false; //disable animation
        state = RagdollState.ragdolled;
    }

    public void DeactivateRagdoll()
    {
        //Transition from ragdolled to animated through the blendToAnim state
        //setKinematic(true); //disable gravity etc.
        ragdollingEndTime = Time.time; //store the state change time
        anim.enabled = true; //enable animation
        state = RagdollState.blendToAnim;

        //Store the ragdolled position for blending
        foreach (BodyPart b in bodyParts)
        {
            b.storedRotation = b.transform.rotation;
            b.storedPosition = b.transform.position;
        }

        //Remember some key positions
        ragdolledFeetPosition = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftToes).position + anim.GetBoneTransform(HumanBodyBones.RightToes).position);
        ragdolledHeadPosition = anim.GetBoneTransform(HumanBodyBones.Head).position;
        ragdolledHipPosition = anim.GetBoneTransform(HumanBodyBones.Hips).position;

        //Initiate the get up animation
        if (anim.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0) //hip hips forward vector pointing upwards, initiate the get up from back animation
        {
            anim.SetTrigger("GetUpFromBack");
            //anim.SetBool("GetUpFromBack", true);
        }
        else
        {
            anim.SetTrigger("GetUpFromBelly");
            //anim.SetBool("GetUpFromBelly", true);
        }
    }

    void setKinematic(bool newValue)
    {
        foreach (Rigidbody rb in rigidBodies)
        {
            if (newValue && rb.collisionDetectionMode == CollisionDetectionMode.Continuous)
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            else if (!newValue && rb.collisionDetectionMode == CollisionDetectionMode.ContinuousSpeculative)
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.isKinematic = newValue;
        }
    }
}
