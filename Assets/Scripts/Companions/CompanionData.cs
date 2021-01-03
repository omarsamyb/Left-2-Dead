using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CompanionData : ScriptableObject
{
    public int MaxClips;
    public int startingClip = 1;
    public string weapon;
    public int ammo;
    public int pistolAmmo = 13;
    public int huntingRifleAmmo = 20;
    public int assultRifleAmmo = 30;
    public string ability;

}
