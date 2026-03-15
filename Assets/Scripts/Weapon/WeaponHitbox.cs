using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public bool active = false;

    void OnTriggerEnter(Collider other)
    {
        if (!active) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy.");
        }
    }
}
