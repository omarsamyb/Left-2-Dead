using UnityEngine;
using System.Collections;

public enum GunStyles
{
    nonautomatic, automatic, shotgun
}
public class GunScript : MonoBehaviour
{
    private MouseLook mouseLook;
    private Transform player;
    private Camera cameraComponent;
    private Transform mainCamera;
    private Camera secondCamera;
    private PlayerController playerController;
    private GunInventory gunInventory;
    private CompanionVoiceOver cvo;
    private Rage rage;
    public string weaponName;
    public Animator handsAnimator;
    private string reloadingAnimationName = "Reloading";
    private string meleeAnimationName = "Melee";
    private string takeoutAnimationName = "Weapon_TakeOut";
    private string takedownAnimationName = "Weapon_TakeDown";

    [Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
    public GameObject[] muzzelFlash;
    [Tooltip("Place on the gun where muzzel flash will appear.")]
    public GameObject muzzelSpawn;

    [Tooltip("Audios for shootingSound, and reloading.")]
    public AudioClip shootSFX, reloadSFX, readySFX;

    [Header("Effects")]
    public GameObject bloodEffect;
    public GameObject wallDecalEffect;

    [Header("Shooting setup - MUSTDO")]
    [HideInInspector] public Transform bulletSpawnPlace;

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
    public float damageRef;
    public float damage;
    private int meleeDamage = 50;
    private int projectileCount = 10;
    private float shotgunSpread = 10f;
    private float waitTillNextFire;

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

    [HideInInspector] public bool isMelee;
    [HideInInspector] public bool isSwitching;
    [HideInInspector] public bool isReloading;

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

    private TextMesh HUD_bullets;

    RaycastHit hitInfo;
    [Tooltip("Put 'Player' layer here")]
    [Header("Shooting Properties")]
    private LayerMask ignoreLayer;
    Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
    private float rayDetectorMeeleSpace = 0.15f;
    private float offsetStart = 0.05f;

    private float weaponNoiseCoolDownRef = 0.5f;
    private float weaponNoiseCoolDown;
    private float noiseRange = 10f;
    Collider[] hits;
    LayerMask enemyLayer;

    void Awake()
    {
        mouseLook = Camera.main.gameObject.GetComponent<MouseLook>();
        player = Camera.main.transform.parent;
        mainCamera = Camera.main.transform;
        secondCamera = mainCamera.GetChild(0).GetComponent<Camera>();
        cameraComponent = mainCamera.GetComponent<Camera>();
        playerController = player.GetComponent<PlayerController>();
        gunInventory = player.GetComponent<GunInventory>();
        rage = player.GetComponent<Rage>();
        damageRef = damage;

        bulletSpawnPlace = GameObject.FindGameObjectWithTag("BulletSpawn").transform;

        rotationLastY = mouseLook.yRotation;
        rotationLastX = mouseLook.xRotation;

        ignoreLayer = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Weapon"));
    }
    private void Start()
    {
        weaponNoiseCoolDown = weaponNoiseCoolDownRef;
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        cvo = CompanionController.instance.transform.GetComponent<CompanionVoiceOver>();
    }
    void Update()
    {
        Controls();
        AnimationStats();
        WeaponPositioning();
        WeaponRotation();
        CrossHairExpansionWhenWalking();

        if (weaponNoiseCoolDown > 0)
            weaponNoiseCoolDown -= Time.deltaTime;
        if (GameManager.instance.inRageMode)
            damage = damageRef * 2;
        else
            damage = damageRef;
    }

    // Controls
    private void Controls()
    {
        Movement();
        Melee();
        Shooting();
        Reloading();
        Aiming();
    }
    private void Movement()
    {
        if (Input.GetButton("Sprint") && Input.GetAxis("Vertical") > 0 && Input.GetAxisRaw("Fire2") == 0 && !isMelee && Input.GetAxisRaw("Fire1") == 0)
        {
            playerController.currentSpeed = runningSpeed;
            handsAnimator.SetInteger("speed", 2);
        }
        else if (playerController.isMoving)
        {
            playerController.currentSpeed = walkingSpeed;
            handsAnimator.SetInteger("speed", 1);
        }
        else
        {
            playerController.currentSpeed = 0f;
            handsAnimator.SetInteger("speed", 0);
        }
    }
    private void Melee()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isMelee && !isReloading && !gunInventory.isThrowing && !playerController.isDashing)
            MeleeAttack();
    }
    private void Shooting()
    {
        if (!isMelee && !isReloading && !isSwitching && !gunInventory.isThrowing && !playerController.isDashing)
        {
            if (currentStyle == GunStyles.nonautomatic)
            {
                if (Input.GetButtonDown("Fire1"))
                    ShootWeapon();
            }
            else if (currentStyle == GunStyles.automatic)
            {
                if (Input.GetButton("Fire1"))
                    ShootWeapon();
            }
            else
            {
                if (Input.GetButtonDown("Fire1"))
                    ShootWeapon();
            }
        }
        waitTillNextFire -= roundsPerSecond * Time.deltaTime;
    }
    private void Reloading()
    {
        if (Input.GetKeyDown(KeyCode.R) && playerController.currentSpeed < runningSpeed && !isReloading && !isMelee && !gunInventory.isThrowing && !playerController.isDashing)
            StartCoroutine(Reload());
    }
    private void Aiming()
    {
        handsAnimator.SetBool("isAiming", Input.GetButton("Fire2"));
        if (Input.GetAxis("Fire2") != 0 && !isReloading && !isMelee && !isSwitching)
        {
            gunPrecision = gunPrecision_aiming;
            recoilAmount_x = recoilAmount_x_;
            recoilAmount_y = recoilAmount_y_;
            recoilAmount_z = recoilAmount_z_;
            currentGunPosition = Vector3.SmoothDamp(currentGunPosition, aimPlacePosition, ref gunPosVelocity, gunAimTime);
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_aiming, ref cameraZoomVelocity, gunAimTime);
            secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_aiming, ref secondCameraZoomVelocity, gunAimTime);
        }
        else
        {
            gunPrecision = gunPrecision_notAiming;
            recoilAmount_x = recoilAmount_x_non;
            recoilAmount_y = recoilAmount_y_non;
            recoilAmount_z = recoilAmount_z_non;
            currentGunPosition = Vector3.SmoothDamp(currentGunPosition, restPlacePosition, ref gunPosVelocity, gunAimTime);
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_notAiming, ref cameraZoomVelocity, gunAimTime);
            secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_notAiming, ref secondCameraZoomVelocity, gunAimTime);
        }
    }

    // Stats
    private void AnimationStats()
    {
        isMelee = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(meleeAnimationName);
        isSwitching = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(takedownAnimationName) | handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(takeoutAnimationName) | (handsAnimator.IsInTransition(0) & (handsAnimator.GetNextAnimatorStateInfo(0).IsName(takedownAnimationName) | handsAnimator.GetNextAnimatorStateInfo(0).IsName(takeoutAnimationName)));
    }

    // Positioning & Rotations
    void WeaponPositioning()
    {
        transform.position = Vector3.SmoothDamp(transform.position,
            mainCamera.transform.position -
            (mainCamera.transform.right * (currentGunPosition.x + currentRecoilXPos)) +
            (mainCamera.transform.up * (currentGunPosition.y + currentRecoilYPos)) +
            (mainCamera.transform.forward * (currentGunPosition.z + currentRecoilZPos)), ref velV, 0);

        currentRecoilZPos = Mathf.SmoothDamp(currentRecoilZPos, 0, ref velocity_z_recoil, recoilOverTime_z);
        currentRecoilXPos = Mathf.SmoothDamp(currentRecoilXPos, 0, ref velocity_x_recoil, recoilOverTime_x);
        currentRecoilYPos = Mathf.SmoothDamp(currentRecoilYPos, 0, ref velocity_y_recoil, recoilOverTime_y);
    }
    void WeaponRotation()
    {
        rotationDeltaY = mouseLook.yRotation - rotationLastY;
        rotationDeltaX = mouseLook.xRotation - rotationLastX;

        rotationLastY = mouseLook.yRotation;
        rotationLastX = mouseLook.xRotation;

        angularVelocityY = Mathf.Lerp(angularVelocityY, rotationDeltaY, Time.deltaTime * 5);
        angularVelocityX = Mathf.Lerp(angularVelocityX, rotationDeltaX, Time.deltaTime * 5);

        gunWeightX = Mathf.SmoothDamp(gunWeightX, mouseLook.xRotation, ref velocityGunRotate.x, rotationLagTime);
        gunWeightY = Mathf.SmoothDamp(gunWeightY, mouseLook.yRotation, ref velocityGunRotate.y, rotationLagTime);

        transform.rotation = Quaternion.Euler(gunWeightX + (angularVelocityX * forwardRotationAmount.x), gunWeightY + (angularVelocityY * forwardRotationAmount.y), 0);
    }

    // Shooting
    private void ShootWeapon()
    {
        if (waitTillNextFire <= 0 && !isReloading && playerController.currentSpeed < runningSpeed)
        {
            if (bulletsInTheGun > 0)
            {
                int randomNumberForMuzzelFlash = Random.Range(0, 5);
                if (currentStyle == GunStyles.shotgun)
                {
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Vector3 rotation = player.transform.rotation.eulerAngles;
                        Vector3 camRotation = mainCamera.transform.rotation.eulerAngles;
                        camRotation.y = 0f;
                        rotation += camRotation;
                        rotation = new Vector3(rotation.x + Random.Range(-shotgunSpread, shotgunSpread), rotation.y + Random.Range(-shotgunSpread, shotgunSpread), 0f);
                        Bullet(Quaternion.Euler(rotation));
                    }
                }
                else
                {
                    Bullet(bulletSpawnPlace.rotation);
                }

                if (weaponNoiseCoolDown <= 0)
                {
                    weaponNoiseCoolDown = weaponNoiseCoolDownRef;
                    hits = Physics.OverlapBox(transform.position, new Vector3(noiseRange, 1f, noiseRange), Quaternion.identity, enemyLayer);
                    foreach (Collider collider in hits)
                    {
                        collider.GetComponent<EnemyContoller>().hearFire();
                    }
                }

                Instantiate(muzzelFlash[randomNumberForMuzzelFlash], muzzelSpawn.transform.position, muzzelSpawn.transform.rotation * Quaternion.Euler(0, 0, 90), muzzelSpawn.transform);
                AudioManager.instance.Play("ShootSFX");
                RecoilMath();
                waitTillNextFire = 1;
                bulletsInTheGun -= 1;
            }
            else
            {
                StartCoroutine(Reload());
            }
        }
    }
    private void Bullet(Quaternion rotation)
    {
        float infrontOfWallDistance = 0.1f; // Good values is between 0.01 to 0.1
        float maxDistance = 1000000;
        ray1 = new Ray(bulletSpawnPlace.position, rotation * Vector3.forward);
        Debug.DrawRay(ray1.origin, ray1.direction * 20f, Color.red);

        if (Physics.Raycast(bulletSpawnPlace.position, rotation * Vector3.forward, out hitInfo, maxDistance, ~ignoreLayer))
        {
            if (hitInfo.transform.CompareTag("Untagged"))
            {
                Instantiate(wallDecalEffect, hitInfo.point + hitInfo.normal * infrontOfWallDistance, Quaternion.LookRotation(hitInfo.normal));
            }
            else if (hitInfo.transform.CompareTag("Enemy") || hitInfo.transform.CompareTag("SpecialEnemy"))
            {
                EnemyContoller enemy = hitInfo.collider.gameObject.GetComponent<EnemyContoller>();
                enemy.TakeDamage((int)damage, hitInfo.point);
                if (enemy.health <= 0)
                {
                    rage.UpdateRage(hitInfo.transform.tag);
                    CompanionController.instance.killCounter++;
                    // TODO: if special is killed add bile to inventory
                }
            }
            else if (hitInfo.transform.CompareTag("Companion"))
            {
                cvo.FriendlyFire();
            }
        }
    }
    private IEnumerator Reload()
    {
        if (bulletsIHave > 0 && bulletsInTheGun < amountOfBulletsPerLoad && !isReloading)
        {
            isReloading = true;
            if (currentStyle == GunStyles.shotgun)
            {
                handsAnimator.SetBool("isReloading", true);
                handsAnimator.SetTrigger("isReloadingShells");
                while (bulletsInTheGun < amountOfBulletsPerLoad && bulletsIHave > 0)
                {
                    AudioManager.instance.Play("ReloadSFX");
                    yield return new WaitForSeconds(0.54f);
                    bulletsIHave--;
                    bulletsInTheGun++;
                    if (Input.GetButton("Fire1"))
                        break;
                }
                handsAnimator.SetBool("isReloading", false);
            }
            else
            {
                AudioManager.instance.Play("ReloadSFX");
                handsAnimator.SetTrigger("isReloading");
                yield return new WaitUntil(() => handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(reloadingAnimationName));
                yield return new WaitUntil(() => !handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(reloadingAnimationName));

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
            isReloading = false;
        }
        else if (bulletsIHave == 0)
            AudioManager.instance.Play("EmptyClipSFX");
    }
    public void RecoilMath()
    {
        currentRecoilZPos -= recoilAmount_z;
        currentRecoilXPos -= (Random.value - 0.5f) * recoilAmount_x;
        currentRecoilYPos -= (Random.value - 0.5f) * recoilAmount_y;
        mouseLook.wantedXRotation -= Mathf.Abs(currentRecoilYPos * gunPrecision);
        mouseLook.wantedYRotation -= (currentRecoilXPos * gunPrecision);
        expandValues_crosshair += new Vector2(6, 12);
    }

    // Melee
    private void MeleeAttack()
    {
        //middle row
        ray1 = new Ray(bulletSpawnPlace.position + (bulletSpawnPlace.right * offsetStart), bulletSpawnPlace.forward + (bulletSpawnPlace.right * rayDetectorMeeleSpace));
        ray2 = new Ray(bulletSpawnPlace.position - (bulletSpawnPlace.right * offsetStart), bulletSpawnPlace.forward - (bulletSpawnPlace.right * rayDetectorMeeleSpace));
        ray3 = new Ray(bulletSpawnPlace.position, bulletSpawnPlace.forward);
        //upper row
        ray4 = new Ray(bulletSpawnPlace.position + (bulletSpawnPlace.right * offsetStart) + (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward + (bulletSpawnPlace.right * rayDetectorMeeleSpace) + (bulletSpawnPlace.up * rayDetectorMeeleSpace));
        ray5 = new Ray(bulletSpawnPlace.position - (bulletSpawnPlace.right * offsetStart) + (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward - (bulletSpawnPlace.right * rayDetectorMeeleSpace) + (bulletSpawnPlace.up * rayDetectorMeeleSpace));
        ray6 = new Ray(bulletSpawnPlace.position + (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward + (bulletSpawnPlace.up * rayDetectorMeeleSpace));
        //bottom row
        ray7 = new Ray(bulletSpawnPlace.position + (bulletSpawnPlace.right * offsetStart) - (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward + (bulletSpawnPlace.right * rayDetectorMeeleSpace) - (bulletSpawnPlace.up * rayDetectorMeeleSpace));
        ray8 = new Ray(bulletSpawnPlace.position - (bulletSpawnPlace.right * offsetStart) - (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward - (bulletSpawnPlace.right * rayDetectorMeeleSpace) - (bulletSpawnPlace.up * rayDetectorMeeleSpace));
        ray9 = new Ray(bulletSpawnPlace.position - (bulletSpawnPlace.up * offsetStart), bulletSpawnPlace.forward - (bulletSpawnPlace.up * rayDetectorMeeleSpace));

        if (!isMelee == true)
        {
            handsAnimator.SetTrigger("isMelee");
            RaycastMelee();
        }
    }
    private void RaycastMelee()
    {
        if (Physics.Raycast(ray1, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray2, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray3, out hitInfo, 2f, ~ignoreLayer)
              || Physics.Raycast(ray4, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray5, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray6, out hitInfo, 2f, ~ignoreLayer)
              || Physics.Raycast(ray7, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray8, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray9, out hitInfo, 2f, ~ignoreLayer))
        {
            if (hitInfo.transform.CompareTag("Enemy") || hitInfo.transform.CompareTag("SpecialEnemy"))
            {
                hitInfo.collider.gameObject.GetComponent<EnemyContoller>().TakeDamage(meleeDamage, hitInfo.point);
            }
        }
    }

    // GUI
    void OnGUI()
    {
        if (!HUD_bullets)
        {
            try
            {
                HUD_bullets = GameObject.Find("HUD_bullets").GetComponent<TextMesh>();
            }
            catch (System.Exception ex)
            {
                print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
            }
        }
        if (HUD_bullets)
            HUD_bullets.text = bulletsIHave.ToString() + " - " + bulletsInTheGun.ToString();
        DrawCrosshair();
    }
    private void DrawCrosshair()
    {
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, fadeout_value);
        if (Input.GetAxis("Fire2") == 0)
        {
            GUI.DrawTexture(new Rect(vec2(left_pos_crosshair).x + position_x(-expandValues_crosshair.x) + Screen.width / 2, Screen.height / 2 + vec2(left_pos_crosshair).y, vec2(size_crosshair_horizontal).x, vec2(size_crosshair_horizontal).y), vertical_crosshair);//left
            GUI.DrawTexture(new Rect(vec2(right_pos_crosshair).x + position_x(expandValues_crosshair.x) + Screen.width / 2, Screen.height / 2 + vec2(right_pos_crosshair).y, vec2(size_crosshair_horizontal).x, vec2(size_crosshair_horizontal).y), vertical_crosshair);//right
            GUI.DrawTexture(new Rect(vec2(top_pos_crosshair).x + Screen.width / 2, Screen.height / 2 + vec2(top_pos_crosshair).y + position_y(-expandValues_crosshair.y), vec2(size_crosshair_vertical).x, vec2(size_crosshair_vertical).y), horizontal_crosshair);//top
            GUI.DrawTexture(new Rect(vec2(bottom_pos_crosshair).x + Screen.width / 2, Screen.height / 2 + vec2(bottom_pos_crosshair).y + position_y(expandValues_crosshair.y), vec2(size_crosshair_vertical).x, vec2(size_crosshair_vertical).y), horizontal_crosshair);//bottom
        }
    }
    private void CrossHairExpansionWhenWalking()
    {
        if (playerController.currentSpeed > 0f && Input.GetAxis("Fire1") == 0)
        {
            expandValues_crosshair += new Vector2(20, 40) * Time.deltaTime;
            if (playerController.currentSpeed < runningSpeed)
            {
                expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y, 0, 20));
                fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
            }
            else
            {
                fadeout_value = Mathf.Lerp(fadeout_value, 0, Time.deltaTime * 10);
                expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 20), Mathf.Clamp(expandValues_crosshair.y, 0, 40));
            }
        }
        else
        {
            expandValues_crosshair = Vector2.Lerp(expandValues_crosshair, Vector2.zero, Time.deltaTime * 5);
            expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y, 0, 20));
            fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
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
