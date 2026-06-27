using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/ProjectileEffect")]
public class ProjectileEffectSO : WeaponEffectSO
{
    [SerializeField] Rigidbody proj;
    [SerializeField] float force = 100f;
    [SerializeField] float spread = 1f;

    public override void Apply(WeaponContext context)
    {
        LaunchProjectile();
    }

    void LaunchProjectile()
    {
        Transform cam = Camera.main.transform;
        Rigidbody projObj = Instantiate(proj, cam.position, cam.rotation);

        Vector3 dir = cam.forward * force + Random.insideUnitSphere * spread;

        projObj.AddForce(dir, ForceMode.Impulse);
    }
}
