using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    public bool active = false;
    public float knockbackForce = 15f;

    readonly HashSet<Collider> hitTargets = new();

    Vector3 lastPosition;

    void Update()
    {
        lastPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active || !other.CompareTag("Enemy") || hitTargets.Contains(other)) return;

        hitTargets.Add(other);
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        Rigidbody enemyRb = other.attachedRigidbody;
        if (enemyRb != null)
        {
            Vector3 swingDir = (transform.position - lastPosition).normalized;

            if (swingDir == Vector3.zero)
                swingDir = transform.forward;

            enemyRb.AddForceAtPosition(swingDir * knockbackForce, hitPoint, ForceMode.Impulse);
        }
    }

    public void StartSwing()
    {
        active = true;
        hitTargets.Clear();
    }

    public void EndSwing()
    {
        active = false;
    }
}