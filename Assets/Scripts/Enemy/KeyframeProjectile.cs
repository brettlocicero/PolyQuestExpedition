using UnityEngine;

public class KeyframeProjectile : MonoBehaviour
{
    [SerializeField] Rigidbody proj;
    [SerializeField] float force;
    [SerializeField] Transform[] launchSpots;

    Vector3 dirVec;
    Transform target;

    int i = 0;

    void Start()
    {
        target = PlayerInstance.instance.transform;
    }

    public void SetDirection()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dirVec = dir * force + PlayerInstance.instance.GetPlayerVelocity();
    }

    public void LaunchProjectile()
    {
        i %= launchSpots.Length;

        Rigidbody projObj = Instantiate(proj, launchSpots[i].position, Quaternion.identity);

        projObj.transform.forward = dirVec.normalized;
        projObj.AddForce(dirVec, ForceMode.Impulse);

        i++;
    }
}
