using UnityEngine;

public class BoomerVomit : MonoBehaviour
{
    InfectedController enemy;

    private void OnParticleCollision(GameObject other)
    {
        enemy = other.GetComponentInParent<InfectedController>();
        if (other.CompareTag("Player") || enemy)
        {
            try
            {
                Boomer boomer = transform.GetComponentInParent<Boomer>();
                if (!ReferenceEquals(boomer.transform, enemy? enemy.transform : other.transform))
                {
                    if ((boomer.targetType && !other.CompareTag("Player")) || (!boomer.targetType && other.CompareTag("Player")))
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
