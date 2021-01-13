using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform player;
    private float sensitivity;
    private float sensitivityNotAiming;
    private float sensitivityAiming;
    private float mouseX;
    private float mouseY;
    [HideInInspector] public float wantedXRotation;
    [HideInInspector] public float wantedYRotation;
    [HideInInspector] public float xRotation;
    [HideInInspector] public float yRotation;
    private float topAngleView;
    private float bottomAngleView;
    private float yRotationSpeed, xRotationSpeed;
    private float rotationYVelocity, rotationXVelocity;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        sensitivity = 150f;
        sensitivityAiming = 150f;
        sensitivityNotAiming = 300f;
        topAngleView = 60f;
        bottomAngleView = -45f;
        yRotationSpeed = 0.1f;
        xRotationSpeed = 0.1f;
    }

    void Update()
    {
        if (!PlayerController.instance.isGettingPinned && !GameManager.instance.inMenu)
            MouseInput();
    }
    private void FixedUpdate()
    {
        if (!PlayerController.instance.isGettingPinned && !GameManager.instance.inMenu)
        {
            yRotation = Mathf.SmoothDamp(yRotation, wantedYRotation, ref rotationYVelocity, yRotationSpeed);
            xRotation = Mathf.SmoothDamp(xRotation, wantedXRotation, ref rotationXVelocity, xRotationSpeed);
            player.rotation = Quaternion.Euler(0f, yRotation, 0f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void MouseInput()
    {
        mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        if (Input.GetAxis("Fire2") != 0)
            sensitivity = sensitivityAiming;
        else
            sensitivity = sensitivityNotAiming;

        wantedYRotation += mouseX;
        wantedXRotation -= mouseY;
        wantedXRotation = Mathf.Clamp(wantedXRotation, bottomAngleView, topAngleView);
    }

    public void AdjustEulers(Quaternion newRotation)
    {
        yRotation = newRotation.eulerAngles.y;
        wantedYRotation = yRotation;
        xRotation = newRotation.eulerAngles.x;
        wantedXRotation = xRotation;
        wantedXRotation -= 360;
    }
}
