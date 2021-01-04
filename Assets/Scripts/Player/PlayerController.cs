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
    public EnemyContoller criticalEnemy;

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
    }

    void Update()
    {
        if(health>0)
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

        moveDirection = transform.right * x + transform.forward * z;
        if (moveDirection.magnitude > 0.1f)
        {
            motor.Move(moveDirection * currentSpeed * Time.deltaTime);
            isMoving = true;
        }
        else
            isMoving = false;
        // Gravity Effect
        velocity.y += gravity * Time.deltaTime;
        motor.Move(velocity * Time.deltaTime);
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
