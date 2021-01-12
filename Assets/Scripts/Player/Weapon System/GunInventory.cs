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
    private Texture[] icons;
    private float switchWeaponCooldown;
    private List<GameObject> myWeapons = new List<GameObject>();

    [HideInInspector] public int currentGrenadeCounter;
    public List<string> grenadesIHave = new List<string>();
    public List<int> grenadesCounter = new List<int>();
    [HideInInspector] public bool isThrowing;
    public InventoryObject CraftableInventory;
    // bile  molotov  pipe  stun  healthpack
    //  0       1      2     3        4
    private TextMesh HUD_grenades;

    void Awake()
    {
        StartCoroutine(UpdateIconsFromResources());
        StartCoroutine(SpawnWeaponUponStart());
        PopulateWeapons();
    }
    private void Start()
    {
        AudioManager.instance.Play("SwitchWeaponSFX");
    }
    void Update()
    {
        if(GetComponent<PlayerController>().health>0)
        {
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
        if (Input.GetKeyDown(KeyCode.G))
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
            if(grenadesIHave.Count == 0)
                print("wtf man");
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
        Vector3 position = currentGun.transform.GetChild(0).Find("L_arm").GetChild(0).GetChild(0).position;
        GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Grenades/" + grenadesIHave[currentGrenadeCounter].ToString() + ".prefab", typeof(GameObject));
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
            int maxCapacity = ((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Grenades/" + name + ".prefab", typeof(GameObject))).GetComponent<GrenadeScript>().maxCapacity;
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

    void removeFromInvObj(string grenade){
        if(grenade == "Bile Bomb")
            CraftableInventory.container[0].addAmount(-1);
        else if(grenade == "Molotov Cocktail")
            CraftableInventory.container[1].addAmount(-1);
        else if(grenade == "Pipe Bomb")
            CraftableInventory.container[2].addAmount(-1);
        else
            CraftableInventory.container[3].addAmount(-1);
    }
    // GUI
    void OnGUI()
    {
        if (!HUD_grenades)
        {
            try
            {
                HUD_grenades = GameObject.Find("HUD_grenades").GetComponent<TextMesh>();
            }
            catch (System.Exception ex)
            {
                // print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
            }
        }
        if (HUD_grenades && grenadesIHave.Count != 0)
            HUD_grenades.text = grenadesIHave[currentGrenadeCounter].ToString() + " - " + grenadesCounter[currentGrenadeCounter].ToString();
        else if (HUD_grenades)
            HUD_grenades.text = "";

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
