using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MenuStyle{
	horizontal,vertical
}

public class GunInventory : MonoBehaviour {
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

	[Header("Sounds")]
	[Tooltip("Sound of weapon changing.")]
	public AudioSource weaponChanging;

	private Animator currentHAndsAnimator;
	private int currentGunCounter = 0;
	public List<string> gunsIHave = new List<string>();
	private Texture[] icons;
	private float switchWeaponCooldown;

	private List<GameObject> myWeapons = new List<GameObject>();


	void Awake(){
		StartCoroutine(UpdateIconsFromResources());
		StartCoroutine(SpawnWeaponUponStart());
		PopulateWeapons();

		if (gunsIHave.Count == 0)
			print ("No guns in the inventory");
	}
	void Update()
	{
		switchWeaponCooldown += 1 * Time.deltaTime;
		if (switchWeaponCooldown > 1.2f && Input.GetKey(KeyCode.LeftShift) == false)
		{
			Create_Weapon();
		}
	}

	IEnumerator SpawnWeaponUponStart(){
		yield return new WaitForSeconds (0.5f);
		StartCoroutine(Spawn(0, 0));
	}

	IEnumerator UpdateIconsFromResources(){
		yield return new WaitForEndOfFrame ();
		icons = new Texture[gunsIHave.Count];
		for(int i = 0; i < gunsIHave.Count; i++){
			icons[i] = (Texture)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/Weapon Icons/" + gunsIHave[i].ToString() + "_img.png", typeof(Texture));
		}

	}
	private void PopulateWeapons()
	{
		int index = myWeapons.Count;
		for(int i = index; i < gunsIHave.Count; i++)
		{
			GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + gunsIHave[i].ToString() + ".prefab", typeof(GameObject));
			GameObject weapon = (GameObject)Instantiate(resource, transform.position, Quaternion.identity);
			weapon.SetActive(false);
			myWeapons.Add(weapon);
		}
	}
	/*
	 * Returns 1 if Ammo is added successfully.
	 * Returns 0 if we don't have the corresponding Ammo's weapon in the Inventory.
	 * Returns -1 if we already have the Max Capacity of Ammo.
	 */
	public int AddAmmo(string weaponName, int amount)
	{
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
	/*
	 * Returns true if we can successfully pick up the weapon.
	 * Returns false if we aleady have this weapon in our Inventory.
	 */
	public bool AddWeapon(string weaponName)
	{
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

	void Create_Weapon(){
		int prevWeaponCounter = currentGunCounter;
		if(Input.GetAxis("Mouse ScrollWheel") > 0){
			switchWeaponCooldown = 0;

			currentGunCounter++;
			if(currentGunCounter > gunsIHave.Count-1){
				currentGunCounter = 0;
			}
			StartCoroutine(Spawn(prevWeaponCounter, currentGunCounter));
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0){
			switchWeaponCooldown = 0;

			currentGunCounter--;
			if(currentGunCounter < 0){
				currentGunCounter = gunsIHave.Count-1;
			}
			StartCoroutine(Spawn(prevWeaponCounter, currentGunCounter));
		}
		else if (Input.GetKeyDown(KeyCode.C))
		{
			switchWeaponCooldown = 0;

			currentGunCounter++;
			if (currentGunCounter > gunsIHave.Count - 1)
				currentGunCounter = 0;
			StartCoroutine(Spawn(prevWeaponCounter, currentGunCounter));
		}
	}

	IEnumerator Spawn(int prevIndex, int _redniBroj){
		if (weaponChanging)
			weaponChanging.Play ();
		else
			print ("Missing Weapon Changing music clip.");
		if(currentGun)
		{
			currentHAndsAnimator.SetBool("changingWeapon", true);
			yield return new WaitForSeconds(0.8f);
			try
			{
				myWeapons[prevIndex].SetActive(false);
				currentGun = myWeapons[_redniBroj];
				currentGun.SetActive(true);
				AssignHandsAnimator();
			}
			catch
			{
				GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + gunsIHave[_redniBroj].ToString() + ".prefab", typeof(GameObject));
				currentGun = (GameObject)Instantiate(resource, transform.position, Quaternion.identity);
				AssignHandsAnimator();
				myWeapons.Add(currentGun);
			}
		}
		else{
			//GameObject resource = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + gunsIHave[_redniBroj].ToString() + ".prefab", typeof(GameObject));
			//currentGun = (GameObject) Instantiate(resource, transform.position, Quaternion.identity);
			currentGun = myWeapons[_redniBroj];
			currentGun.SetActive(true);
			AssignHandsAnimator();
			//myWeapons.Add(currentGun);
		}
	}

	void AssignHandsAnimator(){
		currentHAndsAnimator = currentGun.GetComponent<GunScript>().handsAnimator;
	}

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

	void DrawCorrespondingImage(int _number){
		string deleteCloneFromName = currentGun.name.Substring(0,currentGun.name.Length - 7);
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
	public void DeadMethod(){
		Destroy (currentGun);
		Destroy (this);
	}

	//#####		RETURN THE SIZE AND POSITION for GUI images
	//(we pass in the percentage and it returns some number to appear in that percentage on the sceen) ##################
	private float position_x(float var){
		return Screen.width * var / 100;
	}
	private float position_y(float var)
	{
		return Screen.height * var / 100;
	}
	private Vector2 vec2(Vector2 _vec2){
		return new Vector2(Screen.width * _vec2.x / 100, Screen.height * _vec2.y / 100);
	}
}
