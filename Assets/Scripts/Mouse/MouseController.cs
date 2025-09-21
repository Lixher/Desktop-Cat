using UnityEngine;

public class MouseController : MonoBehaviour
{
    public float lifetime = 4.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}