using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MenuStyle
{
    horizontal, vertical
}

public class GunInventory : MonoBehaviour
{
    public GameObject currentGun;
    [Header("GUI Gun Preview Variables")]
    [Tooltip("Weapon icons orientation")]
    public MenuStyle menuStyle = MenuStyle.horizontal;
    [Tooltip("Spacing between icons.")]
    public int spacing = 10;
    [Tooltip("Begin position in percetanges of screen.")]
    public Vector2 beginPosition;
    [Tooltip("Size of icon in percetanges of screen.")]
    public Vector2 size;

    private Animator currentHandsAnimator;
    private int currentGunCounter = 0;
    public List<string> gunsIHave = new List<string>();
    private Texture[] icons;
    private float switchWeaponCooldown;
    private List<GameObject> myWeapons = new List<GameObject>();

    void Awake()
    {
        StartCoroutine(UpdateIconsFromResources());
        StartCoroutine(SpawnWeaponUponStart());
        PopulateWeapons();
        AudioManager.instance.Play("SwitchWeaponSFX");
    }
    void Update()
    {
        Controls();
        switchWeaponCooldown += 1 * Time.deltaTime;
    }

    // Controls
    private void Controls()
    {
        if (switchWeaponCooldown > 1.2f && Input.GetKey(KeyCode.LeftShift) == false)
        {
            WeaponSwitching();
        }
    }
    private void WeaponSwitching()
    {
        int prevWeaponCounter = currentGunCounter;
        if (!myWeapons[prevWeaponCounter].GetComponent<GunScript>().isReloading)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                switchWeaponCooldown = 0;
                currentGunCounter++;
                if (currentGunCounter > gunsIHave.Count - 1)
                {
                    currentGunCounter = 0;
                }
                StartCoroutine(ActivateWeapon(prevWeaponCounter, currentGunCounter));
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                switchWeaponCooldown = 0;
                currentGunCounter--;
                if (currentGunCounter < 0)
                {
                    currentGunCounter = gunsIHave.Count - 1;
                }
                StartCoroutine(ActivateWeapon(prevWeaponCounter, currentGunCounter));
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                switchWeaponCooldown = 0;
                currentGunCounter++;
                if (currentGunCounter > gunsIHave.Count - 1)
                    currentGunCounter = 0;
                StartCoroutine(ActivateWeapon(prevWeaponCounter, currentGunCounter));
            }
        }
    }

    // Weapons
    public bool AddWeapon(string weaponName)
    {
        /*
         * Returns true if we can successfully pick up the weapon.
         * Returns false if we aleady have this weapon in our Inventory.
         */
        if (gunsIHave.IndexOf(weaponName) == -1)
        {
            gunsIHave.Add(weaponName);
            PopulateWeapons();
            StartCoroutine(UpdateIconsFromResources());
            Debug.Log("Added Weapon " + weaponName);
            return true;
        }
        Debug.Log("Already have weapon");
        return false;
    }
    public int AddAmmo(string weaponName, int amount)
    {
        /*
         * Returns 1 if Ammo is added successfully.
         * Returns 0 if we don't have the corresponding Ammo's weapon in the Inventory.
         * Returns -1 if we already have the Max Capacity of Ammo.
         */
        int index = gunsIHave.IndexOf(weaponName);
        if (index != -1)
        {
            GunScript weapon = myWeapons[index].GetComponent<GunScript>();
            float currentBulletCount = weapon.bulletsIHave;
            if (currentBulletCount == weapon.maxCapacity)
            {
                Debug.Log("Max Ammo Capacity Reached");
                return -1;
            }
            float newCount = currentBulletCount + amount;
            if (newCount > weapon.maxCapacity)
                newCount = weapon.maxCapacity;
            weapon.bulletsIHave = newCount;
            return 1;
        }
        Debug.Log("We don't have this Ammo's Weapon");
        return 0;
    }
    IEnumerator SpawnWeaponUponStart()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ActivateWeapon(0, 0));
    }
    private void PopulateWeapons()
    {
        int index = myWeapons.Count;
        for (int i = index; i < gunsIHave.Count; i++)
        {
            GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + gunsIHave[i].ToString() + ".prefab", typeof(GameObject));
            GameObject weapon = (GameObject)Instantiate(resource, transform.position, Quaternion.identity);
            weapon.SetActive(false);
            myWeapons.Add(weapon);
        }
    }
    IEnumerator ActivateWeapon(int prevIndex, int currIndex)
    {
        if (currentGun)
        {
            AudioManager.instance.Play("SwitchWeaponSFX");
            Vector3 resetPosition = myWeapons[prevIndex].transform.GetChild(0).localPosition;
            currentHandsAnimator.SetTrigger("isSwitching");
            yield return new WaitForSeconds(0.8f);
            AudioManager.instance.PlayOneShot("ReadySFX");
            try
            {
                myWeapons[prevIndex].SetActive(false);
                myWeapons[prevIndex].transform.GetChild(0).localPosition = resetPosition;
                myWeapons[prevIndex].transform.GetChild(0).localRotation = Quaternion.identity;
                currentGun = myWeapons[currIndex];
                currentGun.SetActive(true);
                currentHandsAnimator = currentGun.GetComponent<GunScript>().handsAnimator;
            }
            catch
            {
                GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + gunsIHave[currIndex].ToString() + ".prefab", typeof(GameObject));
                currentGun = (GameObject)Instantiate(resource, transform.position, Quaternion.identity);
                currentHandsAnimator = currentGun.GetComponent<GunScript>().handsAnimator;
                myWeapons.Add(currentGun);
            }
        }
        else
        {
            currentGun = myWeapons[currIndex];
            currentGun.SetActive(true);
            currentHandsAnimator = currentGun.GetComponent<GunScript>().handsAnimator;
            AudioManager.instance.PlayOneShot("ReadySFX");
        }
        AudioManager.instance.SetClip("ShootSFX", currentGun.GetComponent<GunScript>().shootSFX);
        AudioManager.instance.SetClip("ReloadSFX", currentGun.GetComponent<GunScript>().reloadSFX);
        AudioManager.instance.SetClip("ReadySFX", currentGun.GetComponent<GunScript>().readySFX);
    }

    // GUI
    void OnGUI()
    {
        if (currentGun)
        {
            for (int i = 0; i < gunsIHave.Count; i++)
            {
                DrawCorrespondingImage(i);
            }
        }
    }
    void DrawCorrespondingImage(int _number)
    {
        string deleteCloneFromName = currentGun.name.Substring(0, currentGun.name.Length - 7);
        if (icons.Length == gunsIHave.Count)
        {
            if (menuStyle == MenuStyle.horizontal)
            {
                if (deleteCloneFromName == gunsIHave[_number])
                {
                    GUI.DrawTexture(new Rect(vec2(beginPosition).x + (_number * position_x(spacing)), vec2(beginPosition).y,//position variables
                        vec2(size).x, vec2(size).y),//size
                        icons[_number]);
                }
                else
                {
                    GUI.DrawTexture(new Rect(vec2(beginPosition).x + (_number * position_x(spacing) + 10), vec2(beginPosition).y + 10,//position variables
                        vec2(size).x - 20, vec2(size).y - 20),//size
                        icons[_number]);
                }
            }
            else if (menuStyle == MenuStyle.vertical)
            {
                if (deleteCloneFromName == gunsIHave[_number])
                {
                    GUI.DrawTexture(new Rect(vec2(beginPosition).x, vec2(beginPosition).y + (_number * position_y(spacing)),//position variables
                        vec2(size).x, vec2(size).y),//size
                        icons[_number]);
                }
                else
                {
                    GUI.DrawTexture(new Rect(vec2(beginPosition).x, vec2(beginPosition).y + 10 + (_number * position_y(spacing)),//position variables
                        vec2(size).x - 20, vec2(size).y - 20),//size
                        icons[_number]);
                }
            }
        }
    }
    IEnumerator UpdateIconsFromResources()
    {
        yield return new WaitForEndOfFrame();
        icons = new Texture[gunsIHave.Count];
        for (int i = 0; i < gunsIHave.Count; i++)
        {
            icons[i] = (Texture)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/Weapon Icons/" + gunsIHave[i].ToString() + "_img.png", typeof(Texture));
        }
    }
    private float position_x(float var)
    {
        return Screen.width * var / 100;
    }
    private float position_y(float var)
    {
        return Screen.height * var / 100;
    }
    private Vector2 vec2(Vector2 _vec2)
    {
        return new Vector2(Screen.width * _vec2.x / 100, Screen.height * _vec2.y / 100);
    }
}
