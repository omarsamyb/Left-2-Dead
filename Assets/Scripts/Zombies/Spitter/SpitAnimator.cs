using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitAnimator : MonoBehaviour
{
    public GameObject acid;

    ParticleThrow pt;

    public void ThrowBall()
    {
        pt = acid.GetComponent<ParticleThrow>();
        pt.ReleaseMe();
    }
}
