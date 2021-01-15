using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private Vector3 resetPosition;
    private float speed;

    void Start()
    {
        resetPosition = new Vector3(0f, 0f, 79f);
        speed = 1f;
    }

    void Update()
    {
        if(!MainMenu.instance.inMainMenu)
            transform.Translate(0f, 0f, -speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            transform.position = resetPosition;
        }
    }
}
