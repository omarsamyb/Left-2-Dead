using UnityEngine;
using System.Collections;

public enum GunStyles{
	nonautomatic,automatic,shotgun
}
public class GunScript : MonoBehaviour {
	private MouseLook mouseLook;
	private Transform player;
	private Camera cameraComponent;
	private Transform mainCamera;
	private Camera secondCamera;
	private PlayerController playerController;
	public string weaponName;
	public Animator handsAnimator;
	private string reloadAnimationName = "Player_Reload";
	private string aimingAnimationName = "Player_AImpose";
	private string meeleAnimationName = "Character_Malee";
	[HideInInspector]
	public bool reloading;

	[Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
	public GameObject[] muzzelFlash;
	[Tooltip("Place on the gun where muzzel flash will appear.")]
	public GameObject muzzelSpawn;
	private GameObject holdFlash;
	private GameObject holdSmoke;

	[Tooltip("Audios for shootingSound, and reloading.")]
	public AudioClip shootSFX, reloadSFX;
	[Tooltip("Sound that plays after successful attack bullet hit.")]
	public static AudioSource hitMarker;

	[Header("Blood For Meele Attacks")]
	RaycastHit hit;//stores info of hit;
	[Tooltip("Put your particle blood effect here.")]
	public GameObject bloodEffect;//blod effect prefab;
	private GameObject myBloodEffect;

	[Header("Shooting setup - MUSTDO")]
	[HideInInspector] public GameObject bulletSpawnPlace;
	[Tooltip("Bullet prefab that this weapon will shoot.")]
	public GameObject bullet;

	[Tooltip("Selects type of waepon to shoot rapidly or one bullet per click.")]
	public GunStyles currentStyle;

	[Header("Bullet Properties")]
	[Tooltip("How many bullets availabe outside clip.")]
	public float bulletsIHave = 20;
	[Tooltip("how many bullets inside clip.")]
	public float bulletsInTheGun = 5;
	[Tooltip("How many(MAX) bullets can one magazine carry.")]
	public float amountOfBulletsPerLoad = 5;
	public float maxCapacity = 100;
	[Tooltip("Rounds per second if weapon is set to automatic.")]
	public float roundsPerSecond;
	public float damage;
	private int projectileCount = 10;
	private float shotgunSpread = 10f;
	private float waitTillNextFire;

	[Header("Reload Time")]
	[Tooltip("Time that passes after reloading. Depends on your reload animation length, because reloading can be interrupted via meele attack or running. So any action before this finishes will interrupt reloading.")]
	public float reloadChangeBulletsTime;

	[Header("Player Movement Properties")]
	[Tooltip("Player Walking Speed")]
	public int walkingSpeed = 3;
	[Tooltip("Player Running Speed")]
	public int runningSpeed = 5;

	[Header("Crosshair properties")]
	public Texture horizontal_crosshair;
	public Texture vertical_crosshair;
	public Vector2 top_pos_crosshair = new Vector2(-2.7f, -20.8f);
	public Vector2 bottom_pos_crosshair = new Vector2(-2.7f, -7.61f);
	public Vector2 left_pos_crosshair = new Vector2(-14f, -5f);
	public Vector2 right_pos_crosshair = new Vector2(-5.5f, -5f);
	public Vector2 size_crosshair_vertical = new Vector2(6, 28);
	public Vector2 size_crosshair_horizontal = new Vector2(20, 10);
	[HideInInspector]
	public Vector2 expandValues_crosshair;
	private float fadeout_value = 1;

	private float recoilAmount_z = 0.5f;
	private float recoilAmount_x = 0.5f;
	private float recoilAmount_y = 0.5f;
	[Header("Recoil While Not Aiming")]
	[Tooltip("Recoil amount on Z axis")]
	public float recoilAmount_z_non = 0.02f;
	[Tooltip("Recoil amount on X axis")]
	public float recoilAmount_x_non = 0.01f;
	[Tooltip("Recoil amount on y axis")]
	public float recoilAmount_y_non = 0.01f;
	[Header("Recoil While Aiming")]
	[Tooltip("Recoil amount on Z axis")]
	public float recoilAmount_z_ = 0.01f;
	[Tooltip("Recoil amount on X axis")]
	public float recoilAmount_x_ = 0.005f;
	[Tooltip("Recoil amount on y axis")]
	public float recoilAmount_y_ = 0.005f;
	[HideInInspector] public float velocity_z_recoil, velocity_x_recoil, velocity_y_recoil;
	[Header("Recoil reset time")]
	[Tooltip("The time that takes weapon to get back on its original axis after recoil.(The smaller number the faster it gets back to original position)")]
	[HideInInspector] public float recoilOverTime_z = 0.1f;
	[Tooltip("The time that takes weapon to get back on its original axis after recoil.(The smaller number the faster it gets back to original position)")]
	[HideInInspector] public float recoilOverTime_x = 0.1f;
	[Tooltip("The time that takes weapon to get back on its original axis after recoil.(The smaller number the faster it gets back to original position)")]
	[HideInInspector] public float recoilOverTime_y = 0.1f;
	private float currentRecoilZPos;
	private float currentRecoilXPos;
	private float currentRecoilYPos;
	private Vector3 velV;

	[Header("Rotation")]
	private Vector2 velocityGunRotate;
	private float gunWeightX, gunWeightY;
	[Tooltip("The time waepon will lag behind the camera view best set to '0'.")]
	[HideInInspector] public float rotationLagTime = 0f;
	private float rotationLastY;
	private float rotationDeltaY;
	private float angularVelocityY;
	private float rotationLastX;
	private float rotationDeltaX;
	private float angularVelocityX;
	[Tooltip("Value of forward rotation multiplier.")]
	[HideInInspector] public Vector2 forwardRotationAmount = Vector2.one;

	[HideInInspector]
	public Vector3 currentGunPosition;
	[Header("Gun Positioning")]
	[Tooltip("Vector 3 position from player SETUP for NON AIMING values")]
	[HideInInspector] public Vector3 restPlacePosition = new Vector3(-0.07f, -0.06f, 0.4f);
	[Tooltip("Vector 3 position from player SETUP for AIMING values")]
	[HideInInspector] public Vector3 aimPlacePosition = new Vector3(0f, -0.05f, 0.29f);
	[Tooltip("Time that takes for gun to get into aiming stance.")]
	[HideInInspector] public float gunAimTime = 0.05f;
	private Vector3 gunPosVelocity;
	private float cameraZoomVelocity;
	private float secondCameraZoomVelocity;
	private Vector2 gunFollowTimeVelocity;

	[HideInInspector]
	public bool meeleAttack;
	[HideInInspector]
	public bool aiming;

	[Header("Gun Precision")]
	[Tooltip("Gun rate precision when player is not aiming. This is calculated with recoil.")]
	[HideInInspector] public float gunPrecision_notAiming = 300.0f;
	[Tooltip("Gun rate precision when player is aiming. THis is calculated with recoil.")]
	[HideInInspector] public float gunPrecision_aiming = 100.0f;
	[Tooltip("FOV of first camera when NOT aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	[HideInInspector] public float cameraZoomRatio_notAiming = 60;
	[Tooltip("FOV of first camera when aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	[HideInInspector] public float cameraZoomRatio_aiming = 40;
	[Tooltip("FOV of second camera when NOT aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	[HideInInspector] public float secondCameraZoomRatio_notAiming = 60;
	[Tooltip("FOV of second camera when aiming(ONLY SECOND CAMERA RENDERS WEAPONS")]
	[HideInInspector] public float secondCameraZoomRatio_aiming = 50;
	[HideInInspector]
	public float gunPrecision;

	[Tooltip("HUD bullets to display bullet count on screen. Will be find under name 'HUD_bullets' in scene.")]
	[HideInInspector] public TextMesh HUD_bullets;

	RaycastHit hitInfo;
	private float meleeAttack_cooldown;
	private string currentWeapo;
	[Tooltip("Put 'Player' layer here")]
	[Header("Shooting Properties")]
	private LayerMask ignoreLayer;//to ignore player layer
	Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
	private float rayDetectorMeeleSpace = 0.15f;
	private float offsetStart = 0.05f;
	[Tooltip("Put BulletSpawn gameobject here, palce from where bullets are created.")]
	[HideInInspector]
	public Transform bulletSpawn;
	[HideInInspector] public bool been_to_meele_anim = false;

	void Awake(){
		mouseLook = Camera.main.gameObject.GetComponent<MouseLook>();
		player = Camera.main.transform.parent;
		mainCamera = Camera.main.transform;
		secondCamera = mainCamera.GetChild(0).GetComponent<Camera>();
		cameraComponent = mainCamera.GetComponent<Camera>();
		playerController = player.GetComponent<PlayerController>();

		bulletSpawnPlace = GameObject.FindGameObjectWithTag("BulletSpawn");
		//hitMarker = transform.Find ("hitMarkerSound").GetComponent<AudioSource>();

		rotationLastY = mouseLook.yRotation;
		rotationLastX= mouseLook.xRotation;

		bulletSpawn = bulletSpawnPlace.transform;
		ignoreLayer = 1 << LayerMask.NameToLayer("Player");
	}
	void Update(){
		Animations();
		PositionGun();
		Shooting();
		MeeleAttack();
		Sprint(); 
		CrossHairExpansionWhenWalking();
	}
	void FixedUpdate(){
		RotationGun ();
		MeeleAnimationsStates();
		RaycastForMeleeAttacks();

		//if aiming
		if(Input.GetAxis("Fire2") != 0 && !reloading && !meeleAttack){
			gunPrecision = gunPrecision_aiming;
			recoilAmount_x = recoilAmount_x_;
			recoilAmount_y = recoilAmount_y_;
			recoilAmount_z = recoilAmount_z_;
			currentGunPosition = Vector3.SmoothDamp(currentGunPosition, aimPlacePosition, ref gunPosVelocity, gunAimTime);
			cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_aiming, ref cameraZoomVelocity, gunAimTime);
			secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_aiming, ref secondCameraZoomVelocity, gunAimTime);
		}
		//if not aiming
		else{
			gunPrecision = gunPrecision_notAiming;
			recoilAmount_x = recoilAmount_x_non;
			recoilAmount_y = recoilAmount_y_non;
			recoilAmount_z = recoilAmount_z_non;
			currentGunPosition = Vector3.SmoothDamp(currentGunPosition, restPlacePosition, ref gunPosVelocity, gunAimTime);
			cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_notAiming, ref cameraZoomVelocity, gunAimTime);
			secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_notAiming, ref secondCameraZoomVelocity, gunAimTime);
		}
	}

	void Animations()
	{
		if (handsAnimator)
		{
			reloading = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(reloadAnimationName);
			handsAnimator.SetFloat("walkSpeed", playerController.currentSpeed);
			handsAnimator.SetBool("aiming", Input.GetButton("Fire2"));
			handsAnimator.SetInteger("maxSpeed", Mathf.FloorToInt(playerController.currentSpeed));
			if (Input.GetKeyDown(KeyCode.R) && playerController.currentSpeed < runningSpeed && !reloading && !meeleAttack)
			{
				StartCoroutine(Reload_Animation());
			}
		}
	}
	void CrossHairExpansionWhenWalking(){
		if(playerController.currentSpeed > 0f && Input.GetAxis("Fire1") == 0){
			expandValues_crosshair += new Vector2(20, 40) * Time.deltaTime;
			if(playerController.currentSpeed < runningSpeed){
				expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y,0,20));
				fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
			}
			else{
				fadeout_value = Mathf.Lerp(fadeout_value, 0, Time.deltaTime * 10);
				expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 20), Mathf.Clamp(expandValues_crosshair.y,0,40));
			}
		}
		else{
			expandValues_crosshair = Vector2.Lerp(expandValues_crosshair, Vector2.zero, Time.deltaTime * 5);
			expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y,0,20));
			fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
		}
	}
	void Sprint(){
		if (Input.GetAxis ("Vertical") > 0 && Input.GetAxisRaw ("Fire2") == 0 && meeleAttack == false && Input.GetAxisRaw ("Fire1") == 0) {
			if (Input.GetButton("Sprint")) {
				playerController.currentSpeed = runningSpeed;
			}
			else
			{
				playerController.currentSpeed = walkingSpeed;
			}
		}
		else
		{
			if(playerController.isMoving)
				playerController.currentSpeed = walkingSpeed;
			else
				playerController.currentSpeed = 0f;
		}
	}
	void MeeleAnimationsStates(){
		if (handsAnimator) {
			meeleAttack = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(meeleAnimationName);
			aiming = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(aimingAnimationName);	
		}
	}
	void MeeleAttack(){	

		if(Input.GetKeyDown(KeyCode.Q) && !meeleAttack){			
			StartCoroutine(AnimationMeeleAttack());
		}
	}
	IEnumerator AnimationMeeleAttack(){
		handsAnimator.SetBool("meeleAttack",true);
		yield return new WaitForSeconds(0.1f);
		handsAnimator.SetBool("meeleAttack",false);
	}
	void PositionGun(){
		transform.position = Vector3.SmoothDamp(transform.position,
			mainCamera.transform.position  - 
			(mainCamera.transform.right * (currentGunPosition.x + currentRecoilXPos)) + 
			(mainCamera.transform.up * (currentGunPosition.y+ currentRecoilYPos)) + 
			(mainCamera.transform.forward * (currentGunPosition.z + currentRecoilZPos)),ref velV, 0);

		//pmS.cameraPosition = new Vector3(currentRecoilXPos,currentRecoilYPos, 0);

		currentRecoilZPos = Mathf.SmoothDamp(currentRecoilZPos, 0, ref velocity_z_recoil, recoilOverTime_z);
		currentRecoilXPos = Mathf.SmoothDamp(currentRecoilXPos, 0, ref velocity_x_recoil, recoilOverTime_x);
		currentRecoilYPos = Mathf.SmoothDamp(currentRecoilYPos, 0, ref velocity_y_recoil, recoilOverTime_y);

	}
	void RotationGun(){

		rotationDeltaY = mouseLook.yRotation - rotationLastY;
		rotationDeltaX = mouseLook.xRotation - rotationLastX;

		rotationLastY= mouseLook.yRotation;
		rotationLastX= mouseLook.xRotation;

		angularVelocityY = Mathf.Lerp (angularVelocityY, rotationDeltaY, Time.deltaTime * 5);
		angularVelocityX = Mathf.Lerp (angularVelocityX, rotationDeltaX, Time.deltaTime * 5);

		gunWeightX = Mathf.SmoothDamp (gunWeightX, mouseLook.xRotation, ref velocityGunRotate.x, rotationLagTime);
		gunWeightY = Mathf.SmoothDamp (gunWeightY, mouseLook.yRotation, ref velocityGunRotate.y, rotationLagTime);

		transform.rotation = Quaternion.Euler (gunWeightX + (angularVelocityX*forwardRotationAmount.x), gunWeightY + (angularVelocityY*forwardRotationAmount.y), 0);
	}
	public void RecoilMath(){
		currentRecoilZPos -= recoilAmount_z;
		currentRecoilXPos -= (Random.value - 0.5f) * recoilAmount_x;
		currentRecoilYPos -= (Random.value - 0.5f) * recoilAmount_y;
		mouseLook.wantedXRotation -= Mathf.Abs(currentRecoilYPos * gunPrecision);
		mouseLook.wantedYRotation -= (currentRecoilXPos * gunPrecision);		 

		expandValues_crosshair += new Vector2(6,12);
	}
	void Shooting(){
		if (!meeleAttack) {
			if (currentStyle == GunStyles.nonautomatic) {
				if (Input.GetButtonDown ("Fire1")) {
					ShootMethod ();
				}
			}
			else if (currentStyle == GunStyles.automatic) {
				if (Input.GetButton ("Fire1")) {
					ShootMethod ();
				}
			}
			else
			{
				if (Input.GetButtonDown("Fire1"))
				{
					ShootMethod();
				}
			}
		}
		waitTillNextFire -= roundsPerSecond * Time.deltaTime;
	}
	private void ShootMethod()
	{
		if (waitTillNextFire <= 0 && !reloading && playerController.currentSpeed < runningSpeed)
		{
			if (bulletsInTheGun > 0)
			{
				int randomNumberForMuzzelFlash = Random.Range(0, 5);
				if (bullet) {
					if (currentStyle == GunStyles.shotgun)
					{
						for (int i = 0; i < projectileCount; i++)
						{
							Vector3 rotation = player.transform.rotation.eulerAngles;
							Vector3 camRotation = mainCamera.transform.rotation.eulerAngles;
							camRotation.y = 0f;
							rotation += camRotation;
							rotation = new Vector3(rotation.x + Random.Range(-shotgunSpread, shotgunSpread), rotation.y + Random.Range(-shotgunSpread, shotgunSpread), 0f);
							Instantiate(bullet, bulletSpawnPlace.transform.position, Quaternion.Euler(rotation));
						}
					}
					else
					{
						Instantiate(bullet, bulletSpawnPlace.transform.position, bulletSpawnPlace.transform.rotation);
					}
				}
				else
					print("Missing the bullet prefab");
				holdFlash = Instantiate(muzzelFlash[randomNumberForMuzzelFlash], muzzelSpawn.transform.position /*- muzzelPosition*/, muzzelSpawn.transform.rotation * Quaternion.Euler(0, 0, 90)) as GameObject;
				holdFlash.transform.parent = muzzelSpawn.transform;
				if (shootSFX)
					AudioManager.instance.Play("ShootSFX");
				else
					print("Missing 'Shoot Sound Source'.");

				RecoilMath();

				waitTillNextFire = 1;
				bulletsInTheGun -= 1;
			}
			else
			{
				StartCoroutine(Reload_Animation());
			}
		}

	}
	public static void HitMarkerSound(){
		hitMarker.Play();
	}
	IEnumerator Reload_Animation(){
		if (bulletsIHave > 0 && bulletsInTheGun < amountOfBulletsPerLoad && !reloading)
		{
			if (AudioManager.instance.isPlaying("ReloadSFX") == false && reloadSFX != null)
			{
				AudioManager.instance.Play("ReloadSFX");
			}
			handsAnimator.SetBool("reloading", true);
			yield return new WaitForSeconds(0.5f);
			handsAnimator.SetBool("reloading", false);

			yield return new WaitForSeconds(reloadChangeBulletsTime - 0.5f);
			if (meeleAttack == false && playerController.currentSpeed != runningSpeed)
			{
				//if (player.GetComponent<PlayerMovementScript> ()._freakingZombiesSound)
				//	player.GetComponent<PlayerMovementScript> ()._freakingZombiesSound.Play ();
				//else
				//	print ("Missing Freaking Zombies Sound");

				if (bulletsIHave - amountOfBulletsPerLoad >= 0)
				{
					bulletsIHave -= amountOfBulletsPerLoad - bulletsInTheGun;
					bulletsInTheGun = amountOfBulletsPerLoad;
				}
				else if (bulletsIHave - amountOfBulletsPerLoad < 0)
				{
					float valueForBoth = amountOfBulletsPerLoad - bulletsInTheGun;
					if (bulletsIHave - valueForBoth < 0)
					{
						bulletsInTheGun += bulletsIHave;
						bulletsIHave = 0;
					}
					else
					{
						bulletsIHave -= valueForBoth;
						bulletsInTheGun += valueForBoth;
					}
				}
			}
			else
			{
				AudioManager.instance.Stop("ReloadSFX");
				print("Reload interrupted via meele attack");
			}
		}
		else
			AudioManager.instance.Play("EmptyClipSFX");
	}
	void OnGUI(){
		if(!HUD_bullets){
			try{
				HUD_bullets = GameObject.Find("HUD_bullets").GetComponent<TextMesh>();
			}
			catch(System.Exception ex){
				print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
			}
		}
		if(/*mls && */HUD_bullets)
			HUD_bullets.text = bulletsIHave.ToString() + " - " + bulletsInTheGun.ToString();
		DrawCrosshair();
	}
	void DrawCrosshair(){
		GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, fadeout_value);
		if(Input.GetAxis("Fire2") == 0){//if not aiming draw
			GUI.DrawTexture(new Rect(vec2(left_pos_crosshair).x + position_x(-expandValues_crosshair.x) + Screen.width/2,Screen.height/2 + vec2(left_pos_crosshair).y, vec2(size_crosshair_horizontal).x, vec2(size_crosshair_horizontal).y), vertical_crosshair);//left
			GUI.DrawTexture(new Rect(vec2(right_pos_crosshair).x + position_x(expandValues_crosshair.x) + Screen.width/2,Screen.height/2 + vec2(right_pos_crosshair).y, vec2(size_crosshair_horizontal).x, vec2(size_crosshair_horizontal).y), vertical_crosshair);//right

			GUI.DrawTexture(new Rect(vec2(top_pos_crosshair).x + Screen.width/2,Screen.height/2 + vec2(top_pos_crosshair).y + position_y(-expandValues_crosshair.y), vec2(size_crosshair_vertical).x, vec2(size_crosshair_vertical).y ), horizontal_crosshair);//top
			GUI.DrawTexture(new Rect(vec2(bottom_pos_crosshair).x + Screen.width/2,Screen.height/2 +vec2(bottom_pos_crosshair).y + position_y(expandValues_crosshair.y), vec2(size_crosshair_vertical).x, vec2(size_crosshair_vertical).y), horizontal_crosshair);//bottom
		}

	}
	//#####		RETURN THE SIZE AND POSITION for GUI images ##################
	private float position_x(float var){
		return Screen.width * var / 100;
	}
	private float position_y(float var)
	{
		return Screen.height * var / 100;
	}
	private float size_x(float var)
	{
		return Screen.width * var / 100;
	}
	private float size_y(float var)
	{
		return Screen.height * var / 100;
	}
	private Vector2 vec2(Vector2 _vec2){
		return new Vector2(Screen.width * _vec2.x / 100, Screen.height * _vec2.y / 100);
	}
	private void RaycastForMeleeAttacks()
	{
		if (meleeAttack_cooldown > -5)
		{
			meleeAttack_cooldown -= 1 * Time.deltaTime;
		}
		if (player.GetComponent<GunInventory>().currentGun)
		{
			if (player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>())
				currentWeapo = "gun";
		}
		//middle row
		ray1 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace));
		ray2 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace));
		ray3 = new Ray(bulletSpawn.position, bulletSpawn.forward);
		//upper row
		ray4 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart) + (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray5 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart) + (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
		ray6 = new Ray(bulletSpawn.position + (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.up * rayDetectorMeeleSpace));
		//bottom row
		ray7 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart) - (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray8 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart) - (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
		ray9 = new Ray(bulletSpawn.position - (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.up * rayDetectorMeeleSpace));

		Debug.DrawRay(ray1.origin, ray1.direction, Color.cyan);
		Debug.DrawRay(ray2.origin, ray2.direction, Color.cyan);
		Debug.DrawRay(ray3.origin, ray3.direction, Color.cyan);
		Debug.DrawRay(ray4.origin, ray4.direction, Color.red);
		Debug.DrawRay(ray5.origin, ray5.direction, Color.red);
		Debug.DrawRay(ray6.origin, ray6.direction, Color.red);
		Debug.DrawRay(ray7.origin, ray7.direction, Color.yellow);
		Debug.DrawRay(ray8.origin, ray8.direction, Color.yellow);
		Debug.DrawRay(ray9.origin, ray9.direction, Color.yellow);

		if (player.GetComponent<GunInventory>().currentGun)
		{
			if (player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack == false)
			{
				been_to_meele_anim = false;
			}
			if (player.GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack == true && been_to_meele_anim == false)
			{
				been_to_meele_anim = true;
				StartCoroutine(MeeleAttackWeaponHit());
			}
		}
	}
	IEnumerator MeeleAttackWeaponHit()
	{
		if (Physics.Raycast(ray1, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray2, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray3, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast(ray4, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray5, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray6, out hitInfo, 2f, ~ignoreLayer)
			|| Physics.Raycast(ray7, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray8, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray9, out hitInfo, 2f, ~ignoreLayer))
		{
			if (hitInfo.transform.tag == "Enemy")
			{
				Transform _other = hitInfo.transform.root.transform;
				if (_other.transform.tag == "Enemy")
				{
					print("hit an Enemy");
				}
				InstantiateBlood(hitInfo, false);
			}
		}
		yield return new WaitForEndOfFrame();
	}

	void InstantiateBlood(RaycastHit _hitPos, bool swordHitWithGunOrNot)
	{

		if (currentWeapo == "gun")
		{
			GunScript.HitMarkerSound();

			//if (_hitSound)
			//	_hitSound.Play();
			//else
			//	print("Missing hit sound");

			if (!swordHitWithGunOrNot)
			{
				if (bloodEffect)
					Instantiate(bloodEffect, _hitPos.point, Quaternion.identity);
				else
					print("Missing blood effect prefab in the inspector.");
			}
		}
	}
}
