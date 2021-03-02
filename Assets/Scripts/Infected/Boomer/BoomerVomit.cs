using UnityEngine;

public class BoomerVomit : MonoBehaviour
{
    Transform collidee;

    private void OnParticleCollision(GameObject other)
    {
        collidee = other.transform.root;
        if (collidee.CompareTag("Player") || collidee.CompareTag("Enemy") || collidee.CompareTag("SpecialEnemy"))
        {
            try
            {
                Boomer boomer = transform.root.GetComponent<Boomer>();
                if (!ReferenceEquals(boomer.transform, other.transform.root))
                {
                    if ((boomer.targetType && !collidee.CompareTag("Player")) || (!boomer.targetType && collidee.CompareTag("Player")))
                        return;
                    boomer.collided = true;
                }
            }
            catch
            {
                Debug.LogWarning("Boomer died");
            }
        }
    }
}
