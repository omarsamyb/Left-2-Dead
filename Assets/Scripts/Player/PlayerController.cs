using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PathCreation;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [HideInInspector] public GameObject player;
    private CharacterController motor;
    private float x;
    private float z;
    private Vector3 moveDirection;
    [HideInInspector] public float currentSpeed;
    private Vector3 velocity;
    private float gravity;
    public Transform groundCheck;
    private float groundDistance;
    private LayerMask groundMask;
    [HideInInspector] public bool isGrounded;
    private float jumpHeight;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public int health;
    public GameObject character;
    private float dashSpeed;
    private float dashResetSpeed;
    private float dashLength;
    private float dashResetLength;
    private float dashSmoothTime;
    private float dashSpeedRef;
    private float dashTime;
    [HideInInspector] public bool isDashing;
    private GunInventory weaponInventory;
    [HideInInspector] public bool isPinned;
    [HideInInspector] public bool isPartiallyPinned;
    [HideInInspector] public EnemyContoller criticalEnemy;
    private Animation characterAnimation;
    private Rage rage;
    private float addHealthTime;
    public GameObject bileEffect;
    private float bileVisionTimeRef = 4f;
    private float bileVisionTime = 4f;
    public Image damagedEffect;
    public Image criticallyDamagedEffect;
    private float damageFirstFadeDuration = 0.5f;
    private float damageSecondFadeDuration = 1f;
    private float damageFadeTime;
    private float criticallyDamagedFadeDuration = 0.4f;
    private float criticallyDamagedFadeTime;
    private Coroutine damageFadeRoutine;
    public HealthBar healthBar;
    private MouseLook mouseLook;
    [HideInInspector] public bool isGettingPinned;
    public AudioClip[] damagedSFX;
    public AudioClip deathSFX;
    private PlayerVoiceOver pvo;
    private CompanionVoiceOver cvo;
    private int damagedIndex;
    [SerializeField] private AudioClip[] steppingSFX;
    private float previousSpeed;
    
    private void Awake()
    {
        instance = this;
        player = gameObject;
    }
    void Start()
    {
        motor = GetComponent<CharacterController>();
        mouseLook = Camera.main.gameObject.GetComponent<MouseLook>();
        currentSpeed = 0f;
        gravity = -9.81f * 2f;
        groundDistance = 0.4f;
        jumpHeight = 1f;
        groundMask = 1 << LayerMask.NameToLayer("Ground");
        isGrounded = true;
        isMoving = false;
        health = 300;
        healthBar.SetMaxHealth(health);
        dashSpeed = 25f;
        dashResetSpeed = 3f;
        dashLength = 0.08f;
        dashResetLength = 0.6f;
        dashSmoothTime = 0.02f;
        weaponInventory = GetComponent<GunInventory>();
        characterAnimation = character.GetComponent<Animation>();
        rage = GetComponent<Rage>();
        addHealthTime = 1;
        pvo = GetComponent<PlayerVoiceOver>();
        cvo = CompanionController.instance.transform.GetComponent<CompanionVoiceOver>();
    }

    void Update()
    {
        if (health > 0)
        {
            if (!isPinned && !isPartiallyPinned)
                PlayerMovement();

            if (GameManager.instance.companionId == 2)
            {
                addHealthTime -= Time.deltaTime;
                if (addHealthTime <= 0)
                {
                    addHealthTime = 1f;
                    if (CompanionController.instance.canApplyAbility)
                        AddHealth(1);
                }
            }

            SteppingSFX();
        }

        if (bileEffect.activeSelf)
        {
            bileVisionTime -= Time.deltaTime;
            if (bileVisionTime <= 0)
                bileEffect.SetActive(false);
        }
    }

    // Controls
    private void PlayerMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump") && isGrounded && !isDashing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            if (Input.GetButtonDown("Dash") && CanDoIt())
            {
                StartCoroutine(Dash(moveDirection));
            }
            if (!isDashing && !isPinned)
            {
                motor.Move(moveDirection * currentSpeed * Time.deltaTime);
                isMoving = true;
            }
        }
        else
            isMoving = false;
        // Gravity Effect
        velocity.y += gravity * Time.deltaTime;
        motor.Move(velocity * Time.deltaTime);
    }
    IEnumerator Dash(Vector3 direction)
    {
        if (Mathf.Abs(Mathf.Abs(direction.x) - Mathf.Abs(direction.z)) < 0.1f)
        {
            weaponInventory.currentHandsAnimator.SetFloat("dashZ", direction.z);
            weaponInventory.currentHandsAnimator.SetFloat("dashX", 0.0f);
        }
        else if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            weaponInventory.currentHandsAnimator.SetFloat("dashX", direction.x);
            weaponInventory.currentHandsAnimator.SetFloat("dashZ", 0.0f);
        }
        else
        {
            weaponInventory.currentHandsAnimator.SetFloat("dashZ", direction.z);
            weaponInventory.currentHandsAnimator.SetFloat("dashX", 0.0f);
        }

        isDashing = true;
        weaponInventory.currentHandsAnimator.SetTrigger("isDashing");

        while (!weaponInventory.currentHandsAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Dashing"))
        {
            yield return new WaitForEndOfFrame();
        }
        AudioManager.instance.Play("DashSFX");

        dashTime = 0f;
        float currentDashSpeed = currentSpeed;
        while (dashTime < dashLength)
        {
            currentDashSpeed = Mathf.SmoothDamp(currentDashSpeed, dashSpeed, ref dashSpeedRef, dashSmoothTime);
            motor.Move(direction * currentDashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        dashTime = 0f;
        while (dashTime < dashResetLength)
        {
            currentDashSpeed = Mathf.SmoothDamp(currentDashSpeed, dashResetSpeed, ref dashSpeedRef, dashSmoothTime);
            motor.Move(direction * currentDashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isDashing = false;
    }
    public bool CanDoIt()
    {
        return isGrounded && !isDashing && !isPinned && !isPartiallyPinned && weaponInventory.currentGun && !weaponInventory.currentGun.GetComponent<GunScript>().isSwitching && !weaponInventory.currentGun.GetComponent<GunScript>().isReloading && !weaponInventory.currentGun.GetComponent<GunScript>().isMelee;
    }

    // Health
    private void Die()
    {
        cvo.PlayerDeath();
        AudioManager.instance.Stop("DamagedSFX");
        AudioManager.instance.SetClip("DamagedSFX", deathSFX);
        AudioManager.instance.Play("DamagedSFX");
        character.SetActive(true);
        if (isPinned || isPartiallyPinned)
            characterAnimation.Play("Die_Pinned");
        else
        {
            characterAnimation.Play("Die");
            Vector3 camVector = Camera.main.transform.position - Camera.main.transform.forward * 2f;
            camVector.y = 0f;
            camVector += Vector3.up;
            Camera.main.transform.position = camVector;
        }
        character.transform.parent = null;
        GameManager.instance.isGameOver = true;
    }
    public void AddHealth(int points)
    {
        health = Mathf.Clamp(health + points, 0, 300);
    }
    public void TakeDamage(int points)
    {
        if (rage.canBeDamaged && health > 0)
        {
            health -= points;
            healthBar.SetHealth(health);
            if (health <= 0)
                Die();
            else if(!AudioManager.instance.isPlaying("DamagedSFX"))
            {
                AudioClip clip = damagedSFX[damagedIndex];
                AudioManager.instance.SetClip("DamagedSFX", clip);
                AudioManager.instance.Play("DamagedSFX");
                damagedIndex = (damagedIndex + 1) % damagedSFX.Length;
            }

            if (damageFadeRoutine != null)
                StopCoroutine(damageFadeRoutine);
            damageFadeRoutine = StartCoroutine(DamagedEffect());

            if (health < 50) {
                cvo.HealUp();
                if(!AudioManager.instance.isPlaying("CriticallyDamagedSFX"))
                    StartCoroutine(CriticallyDamagedEffect());
            }
        }
    }
   
    // Pinning
    public void GetPinned(GameObject enemy)
    {
        isPinned = true;
        isMoving = false;
        criticalEnemy = enemy.GetComponent<EnemyContoller>();
        StartCoroutine(GetPinnedHelper());
    }
    IEnumerator GetPinnedHelper()
    {
        weaponInventory.currentHandsAnimator.SetTrigger("reset");
        yield return new WaitForEndOfFrame();
        weaponInventory.currentGun.SetActive(false);
        character.SetActive(true);
        characterAnimation.Play("Pinned");
        Vector3 origPos = Camera.main.transform.localPosition;
        Vector3 camVector = Camera.main.transform.position - Camera.main.transform.forward * 2f;
        camVector.y = 0f;
        camVector += Vector3.up;
        Camera.main.transform.position = camVector;
        Transform origParent = character.transform.parent;
        character.transform.parent = null;
        while (isPinned)
            yield return null;
        pvo.StartCoroutine(pvo.Unpinned(1));
        characterAnimation.Stop("Pinned");
        character.SetActive(false);
        character.transform.SetParent(origParent);
        Camera.main.transform.localPosition = origPos;
        weaponInventory.currentGun.SetActive(true);
    }
    public void GetPartiallyPinned(Vector3 topPoint, Vector3 endPoint, float speed, Vector3 poi)
    {
        isPartiallyPinned = true;
        isPinned = true;
        isGettingPinned = true;
        isMoving = false;
        Vector3[] points = { transform.position, topPoint, endPoint };
        BezierPath bezierPath = new BezierPath(points);
        GameObject placeholder = new GameObject("PinningPathPlaceHolder");
        StartCoroutine(GetPartiallyPinnedHelper(new VertexPath(bezierPath, placeholder.transform), speed, poi));
    }
    IEnumerator GetPartiallyPinnedHelper(VertexPath path, float speed, Vector3 poi)
    {
        pvo.StartCoroutine(pvo.Pinned());
        weaponInventory.currentHandsAnimator.SetTrigger("reset");
        yield return new WaitForEndOfFrame();
        weaponInventory.currentGun.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        AudioManager.instance.Play("ScreamingSFX");
        character.transform.SetParent(Camera.main.transform);
        character.transform.Translate(0f, 0f, -0.12f);
        character.transform.Rotate(-3f, 0f, 0f);
        character.SetActive(true);
        characterAnimation.Play("Falling");
        motor.enabled = false;
        yield return null;
        Time.timeScale = 0.3f;

        float distanceTravelled = 0f;
        while(distanceTravelled < path.length)
        {
            distanceTravelled += speed * Time.deltaTime;
            Camera.main.transform.rotation = Quaternion.LookRotation(poi - transform.position);
            transform.position = path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
            yield return new WaitForEndOfFrame();
        }
        AudioManager.instance.Play("HitGroundSFX");
        AudioManager.instance.Stop("ScreamingSFX");
        mouseLook.AdjustEulers(Camera.main.transform.rotation);
        yield return null;
        characterAnimation.Stop("Falling");
        character.SetActive(false);
        character.transform.SetParent(transform);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.identity;
        weaponInventory.currentGun.SetActive(true);
        isGettingPinned = false;
        Time.timeScale = 1f;
        while (isPartiallyPinned)
            yield return null;
        pvo.StartCoroutine(pvo.Unpinned(0));
        Vector3 resetY = transform.position;
        resetY.y += 2;
        transform.position = resetY;

        isPartiallyPinned = false;
        motor.enabled = true;
        isPinned = false;
    }

    // Effects
    IEnumerator DamagedEffect()
    {
        Color color = damagedEffect.color;
        color.a = 1f;
        damagedEffect.color = color;
        damageFadeTime = 0f;
        while (damageFadeTime < damageFirstFadeDuration)
        {
            damageFadeTime += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0.5f, damageFadeTime / damageFirstFadeDuration);
            damagedEffect.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        damageFadeTime = 0f;
        while (damageFadeTime < damageSecondFadeDuration)
        {
            damageFadeTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.5f, 0f, damageFadeTime / damageFirstFadeDuration);
            damagedEffect.color = color;
            yield return null;
        }
    }
    IEnumerator CriticallyDamagedEffect()
    {
        AudioManager.instance.Play("CriticallyDamagedSFX");
        Color color = criticallyDamagedEffect.color;
        color.a = 1f;
        criticallyDamagedEffect.color = color;

        while (health < 50)
        {
            criticallyDamagedFadeTime = 0f;
            while (criticallyDamagedFadeTime < criticallyDamagedFadeDuration)
            {
                if (health <= 0)
                    break;
                criticallyDamagedFadeTime += Time.deltaTime;
                color.a = Mathf.Lerp(1, 0.2f, criticallyDamagedFadeTime / criticallyDamagedFadeDuration);
                criticallyDamagedEffect.color = color;
                yield return null;
            }
            if (health <= 0)
                break;
            criticallyDamagedFadeTime = 0f;
            while (criticallyDamagedFadeTime < criticallyDamagedFadeDuration)
            {
                if (health <= 0)
                    break;
                criticallyDamagedFadeTime += Time.deltaTime;
                color.a = Mathf.Lerp(0.2f, 1f, criticallyDamagedFadeTime / criticallyDamagedFadeDuration);
                criticallyDamagedEffect.color = color;
                yield return null;
            }
        }
        AudioManager.instance.Stop("CriticallyDamagedSFX");
        color.a = 0f;
        criticallyDamagedEffect.color = color;
    }
    private void SteppingSFX()
    {
        if (currentSpeed == 0f)
        {
            AudioManager.instance.Stop("SteppingSFX");
            return;
        }
        if (!AudioManager.instance.isPlaying("SteppingSFX") || previousSpeed != currentSpeed)
        {
            if (currentSpeed == 5f)
            {
                AudioManager.instance.SetClip("SteppingSFX", steppingSFX[1]);
                AudioManager.instance.Play("SteppingSFX");
            }
            else if (currentSpeed == 3f)
            {
                AudioManager.instance.SetClip("SteppingSFX", steppingSFX[0]);
                AudioManager.instance.Play("SteppingSFX");
            }
        }
        previousSpeed = currentSpeed;
    }
    public void BileVisionEffect()
    {
        pvo.StartCoroutine(pvo.Bile());
        bileEffect.SetActive(true);
        bileVisionTime = bileVisionTimeRef;
        AudioManager.instance.Play("BileEffectSFX");
    }
}
