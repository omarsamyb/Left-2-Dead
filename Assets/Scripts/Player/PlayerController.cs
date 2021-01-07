using System.Collections;
using UnityEngine;

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
    private bool isGrounded;
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
    public EnemyContoller criticalEnemy;

    private void Awake()
    {
        instance = this;
        player = gameObject;
    }
    void Start()
    {
        motor = GetComponent<CharacterController>();
        currentSpeed = 0f;
        gravity = -9.81f * 2f;
        groundDistance = 0.4f;
        jumpHeight = 1f;
        groundMask = 1 << LayerMask.NameToLayer("World");
        isGrounded = true;
        isMoving = false;
        health = 300;
        dashSpeed = 25f;
        dashResetSpeed = 3f;
        dashLength = 0.08f;
        dashResetLength = 0.6f;
        dashSmoothTime = 0.02f;
        weaponInventory = GetComponent<GunInventory>();
    }

    void Update()
    {
        if (health > 0)
            PlayerMovement();
    }

    private void PlayerMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump") && isGrounded && !isDashing && !isPinned)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            if (Input.GetButtonDown("Dash") && isGrounded && !isDashing && !isPinned && weaponInventory.currentGun && !weaponInventory.currentGun.GetComponent<GunScript>().isSwitching && !weaponInventory.currentGun.GetComponent<GunScript>().isReloading && !weaponInventory.currentGun.GetComponent<GunScript>().isMelee)
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
        if(Mathf.Abs(Mathf.Abs(direction.x) - Mathf.Abs(direction.z)) < 0.1f)
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
    public void TakeDamage(int points)
    {
        health -= points;
        if (health <= 0)
            Die();
    }
    private void Die()
    {
        character.SetActive(true);
        Camera.main.transform.position -= Camera.main.transform.forward + Camera.main.transform.up;
        character.transform.parent = null;
    }
}
