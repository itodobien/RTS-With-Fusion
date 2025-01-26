using UnityEngine;

public class DestructibleCrate : MonoBehaviour
{
    public void Damage()
    {
        Destroy(gameObject);
    }
}