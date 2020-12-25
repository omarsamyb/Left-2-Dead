using UnityEngine;

public class DestroyAfterTimeParticle : MonoBehaviour {
	[Tooltip("Time to destroy")]
	public float timeToDestroy = 0.8f;

	void Start () {
		Destroy (gameObject, timeToDestroy);
	}
}
