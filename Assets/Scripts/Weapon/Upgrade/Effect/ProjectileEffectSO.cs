using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/ProjectileEffect")]
public class ProjectileEffectSO : WeaponEffectSO
{
    [SerializeField] Rigidbody proj;
    [SerializeField] float projForce = 100f;

    public override void Apply(WeaponContext context)
    {
        Vector3 launchPos = context.weapon.transform.position;

        Transform camTform = Camera.main.transform;
        Rigidbody projObj = Instantiate(proj, launchPos, camTform.rotation);

        Vector3 dir = camTform.forward;
        projObj.AddForce(dir * projForce, ForceMode.Impulse);
    }
}
