using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
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
    private bool isDashing;

    //Vector3 direction;

    private void Awake()
    {
        motor = GetComponent<CharacterController>();
    }
    void Start()
    {
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
        dashResetLength = 0.3f;
        dashSmoothTime = 0.02f;
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
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isDashing)
            {
                StartCoroutine(Dash(moveDirection));
            }
            if (!isDashing)
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
        isDashing = true;
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
        while(dashTime < dashResetLength)
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
