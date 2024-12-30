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

    [HideInInspector] public Animator currentHandsAnimator;
    private int currentGunCounter = 0;
    public List<string> gunsIHave = new List<string>();
    private float switchWeaponCooldown;
    private List<GameObject> myWeapons = new List<GameObject>();

    [HideInInspector] public int currentGrenadeCounter;
    public List<string> grenadesIHave = new List<string>();
    public List<int> grenadesCounter = new List<int>();
    [HideInInspector] public bool isThrowing;
    public InventoryObject CraftableInventory;
    // bile  molotov  pipe  stun  healthpack
    //  0       1      2     3        4
    public GameObject[] grenadePrefabs;
    public GameObject[] weaponPrefabs;

    private void Start()
    {
        AudioManager.instance.Play("SwitchWeaponSFX");
        StartCoroutine(SpawnWeaponUponStart());
    }
    void Update()
    {
        if (GetComponent<PlayerController>().health > 0)
        {
            if (!GameManager.instance.inMenu)
                Controls();
            switchWeaponCooldown += 1 * Time.deltaTime;
        }
        else
        {
            currentGun.SetActive(false);
        }
    }

    // Controls
    private void Controls()
    {
        if (currentGun)
        {
            GunScript gunState = currentGun.GetComponent<GunScript>();
            if (switchWeaponCooldown > 1.2f && Input.GetKey(KeyCode.LeftShift) == false && !gunState.isReloading && !gunState.isMelee && !isThrowing && !gunState.isSwitching)
            {
                WeaponSwitching();
                Grenade();
            }
        }
    }
    private void WeaponSwitching()
    {
        if (currentGun.activeSelf)
        {
            int prevWeaponCounter = currentGunCounter;
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
    private void Grenade()
    {
        if (Input.GetKeyDown(KeyCode.G) && PlayerController.instance.CanDoIt())
        {
            StartCoroutine(InitiateThrow());
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentGrenadeCounter++;
            if (currentGrenadeCounter > grenadesIHave.Count - 1)
                currentGrenadeCounter = 0;
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
            return true;
        }
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
                return -1;
            }
            float newCount = currentBulletCount + amount;
            if (newCount > weapon.maxCapacity)
                newCount = weapon.maxCapacity;
            weapon.bulletsIHave = newCount;
            return 1;
        }
        return 0;
    }
    IEnumerator SpawnWeaponUponStart()
    {
        yield return new WaitForSeconds(0.5f);
        PopulateWeapons();
        StartCoroutine(ActivateWeapon(0, 0));
    }
    private void PopulateWeapons()
    {
        int index = myWeapons.Count;
        for (int i = index; i < gunsIHave.Count; i++)
        {
            GameObject resource = getWeapon(gunsIHave[i].ToString());
            GameObject weapon = (GameObject)Instantiate(resource, transform.position, Quaternion.identity);
            weapon.SetActive(false);
            myWeapons.Add(weapon);
        }
    }
    GameObject getWeapon(string name)
    {
        string weaponName = name;
        if (weaponName == "Assault Rifle")
            return weaponPrefabs[0];
        else if (weaponName == "Hunting Rifle")
            return weaponPrefabs[1];
        else if (weaponName == "Pistol")
            return weaponPrefabs[2];
        else if (weaponName == "Submachine Gun")
            return weaponPrefabs[3];
        else if (weaponName == "Tactical Shotgun")
            return weaponPrefabs[4];
        else if (weaponName == "Rocket Launcher")
            return weaponPrefabs[5];
        return null;
    }
    GameObject getGrenade(string name)
    {
        if (name == "Bile Bomb")
            return grenadePrefabs[0];
        else if (name == "Molotov Cocktail")
            return grenadePrefabs[1];
        else if (name == "Pipe Bomb")
            return grenadePrefabs[2];
        else if (name == "Stun Grenade")
            return grenadePrefabs[3];
        return null;
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
                GameObject resource = getWeapon(gunsIHave[currIndex].ToString());
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

    // Grenades
    private IEnumerator InitiateThrow()
    {
        if (grenadesIHave.Count > currentGrenadeCounter)
        {
            AudioManager.instance.Play("ThrowSFX");
            yield return new WaitForSeconds(0.05f);
            isThrowing = true;
            currentHandsAnimator.SetTrigger("isThrowing");
            yield return new WaitForSeconds(0.07f);
            StartCoroutine(Throw());
            removeFromInvObj(grenadesIHave[currentGrenadeCounter]);

            if (grenadesCounter[currentGrenadeCounter] - 1 == 0)
            {
                grenadesIHave.RemoveAt(currentGrenadeCounter);
                grenadesCounter.RemoveAt(currentGrenadeCounter);
                if (currentGrenadeCounter == grenadesIHave.Count)
                {
                    currentGrenadeCounter--;
                    if (currentGrenadeCounter < 0)
                        currentGrenadeCounter = 0;
                }
            }
            else
                grenadesCounter[currentGrenadeCounter]--;
        }
    }
    private IEnumerator Throw()
    {
        Vector3 position = currentGun.transform.GetChild(0).Find("L_arm").GetChild(0).GetChild(0).position + Camera.main.transform.forward;
        GameObject resource = getGrenade(grenadesIHave[currentGrenadeCounter].ToString());
        Instantiate(resource, position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        isThrowing = false;
    }
    public bool AddGrenade(string name)
    {
        /*
         * Returns true if grenade capacity is not full
         * Returns false otherwise.
         */
        int index = grenadesIHave.IndexOf(name);
        if (index != -1)
        {
            int maxCapacity = getGrenade(name).GetComponent<GrenadeScript>().maxCapacity;
            int value = grenadesCounter[index] + 1;
            if (value > maxCapacity)
                return false;
            else
            {
                grenadesCounter[index] = value;
                return true;
            }
        }
        else
        {
            grenadesIHave.Add(name);
            grenadesCounter.Add(1);
            return true;
        }
    }

    void removeFromInvObj(string grenade)
    {
        if (grenade == "Bile Bomb")
            CraftableInventory.container[0].addAmount(-1);
        else if (grenade == "Molotov Cocktail")
            CraftableInventory.container[1].addAmount(-1);
        else if (grenade == "Pipe Bomb")
            CraftableInventory.container[2].addAmount(-1);
        else
            CraftableInventory.container[3].addAmount(-1);
    }
}
