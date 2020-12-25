using UnityEngine;

public class BulletScript : MonoBehaviour {
	public GameObject decalHitWall;
	public GameObject bloodEffect;

	private float floatInfrontOfWall;
	private RaycastHit hit;
	private float maxDistance;
	private LayerMask ignoreLayer;
	private void Start()
	{
		ignoreLayer = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Weapon"));
		maxDistance = 1000000;
		floatInfrontOfWall = 0.1f;	// Good values is between 0.01 to 0.1
	}
	void Update () {
		Ray ray1 = new Ray(transform.position, transform.forward);
		Debug.DrawRay(ray1.origin, ray1.direction * 20f, Color.red);
		if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, ~ignoreLayer)){
			if(hit.transform.tag == "Untagged"){
				Instantiate(decalHitWall, hit.point + hit.normal * floatInfrontOfWall, Quaternion.LookRotation(hit.normal));
				Destroy(gameObject);
			}
			else if(hit.transform.tag == "Enemy"){
				Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
				Destroy(gameObject);
			}
			Destroy(gameObject);
		}
		Destroy(gameObject, 0.1f);
	}
}
