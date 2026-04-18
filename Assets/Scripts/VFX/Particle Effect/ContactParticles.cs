using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ContactParticles : MonoBehaviour
{
    ParticleSystem particles;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public void InitParticleSystem(WeaponAttack attack)
    {
        var velocityModule = particles.velocityOverLifetime;
        velocityModule.enabled = true;

        velocityModule.x = new ParticleSystem.MinMaxCurve((float)attack.attackDirection * 3f);
    }
}
