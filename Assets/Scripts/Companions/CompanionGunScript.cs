using UnityEngine;

public class CompanionGunScript : MonoBehaviour
{
    public string weaponName;
    public int maxClips = 3;
    public GameObject muzzelSpawn;

    private GameObject[] muzzelFlash;
    private AudioClip shootSFX;
    private GameObject bloodEffect;
    private GameObject wallDecalEffect;
    private GunStyles style;
    private int amountOfBulletsPerLoad;
    private int bulletsIHave;
    private int maxCapacity;
    private float roundsPerSecond;
    private int damage;
    private float waitTillNextFire;
    private RaycastHit hitInfo;
    private AudioSource fireSource;

    void Start()
    {
        GunScript weapon = ((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Weapons/" + weaponName + ".prefab", typeof(GameObject))).GetComponent<GunScript>();
        muzzelFlash = weapon.muzzelFlash;
        shootSFX = weapon.shootSFX;
        bloodEffect = weapon.bloodEffect;
        wallDecalEffect = weapon.wallDecalEffect;
        style = weapon.currentStyle;
        amountOfBulletsPerLoad = (int)weapon.amountOfBulletsPerLoad;
        bulletsIHave = amountOfBulletsPerLoad;
        maxCapacity = amountOfBulletsPerLoad * maxClips;
        roundsPerSecond = weapon.roundsPerSecond;
        damage = (int)weapon.damage;
        fireSource = GetComponent<AudioSource>();
        fireSource.clip = shootSFX;
    }

    void Update()
    {
        Shooting();
    }
    private void Shooting()
    {
        if (style == GunStyles.nonautomatic)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                ShootWeapon();
        }
        else if (style == GunStyles.automatic)
        {
            if (Input.GetKey(KeyCode.Q))
                ShootWeapon();
        }
        waitTillNextFire -= roundsPerSecond * Time.deltaTime;
    }
    private void ShootWeapon()
    {
        if (waitTillNextFire <= 0 && bulletsIHave > 0)
        {
            int randomNumberForMuzzelFlash = Random.Range(0, 5);
            Bullet(muzzelSpawn.transform.rotation);
            Instantiate(muzzelFlash[randomNumberForMuzzelFlash], muzzelSpawn.transform.position, muzzelSpawn.transform.rotation * Quaternion.Euler(0, 0, 90), muzzelSpawn.transform);
            fireSource.Play();
            waitTillNextFire = 1;
            bulletsIHave--;
        }
    }
    private void Bullet(Quaternion rotation)
    {
        float infrontOfWallDistance = 0.1f;
        float maxDistance = 1000000;
        Ray ray = new Ray(muzzelSpawn.transform.position, rotation * Vector3.forward);
        Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red);

        if (Physics.Raycast(muzzelSpawn.transform.position, rotation * Vector3.forward, out hitInfo, maxDistance))
        {
            if (hitInfo.transform.tag == "Untagged")
            {
                Instantiate(wallDecalEffect, hitInfo.point + hitInfo.normal * infrontOfWallDistance, Quaternion.LookRotation(hitInfo.normal));
            }
            else if (hitInfo.transform.tag == "Enemy")
            {
                Instantiate(bloodEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                hitInfo.collider.gameObject.GetComponent<EnemyContoller>().TakeDamage(damage);
            }
        }
    }
    public void AddClip()
    {
        bulletsIHave = Mathf.Clamp(bulletsIHave + amountOfBulletsPerLoad, 0, maxCapacity);
    }
}
