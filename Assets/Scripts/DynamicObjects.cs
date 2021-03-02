using UnityEngine;

public class DynamicObjects : MonoBehaviour
{
    public static DynamicObjects instance;
    private void Awake()
    {
        instance = this;
    }
}
