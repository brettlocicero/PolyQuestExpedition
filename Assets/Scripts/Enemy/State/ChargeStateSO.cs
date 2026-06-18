using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Charge State")]
public class ChargeStateSO : EnemyStateSO
{
    public override System.Type StateType => typeof(ChargeState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new ChargeState(ai, this);
    }
}

public class ChargeState : EnemyBaseState
{
    private float chargeTimer;
    private float maxChargeDuration = 2.0f; // Stop charging if they miss or get stuck
    private float originalSpeed;
    private float originalAcceleration;
    private float chargeSpeedMultiplier = 1.4f; // 40% faster when rushing the player

    public ChargeState(EnemyAI ai, EnemyStateSO definition) : base(ai, definition) {}

    public override void EnterState()
    {
        chargeTimer = 0f;
        originalSpeed = Agent.speed;
        originalAcceleration = Agent.acceleration;
        
        // Boost speed for a dramatic, aggressive rush
        Agent.speed = originalSpeed * chargeSpeedMultiplier;
        Agent.acceleration = originalAcceleration * 2f; // Snappier response
        
        // Optional: Trigger a running/screaming animation if you have one
        // if (AI.anim) AI.anim.SetTrigger("ChargeRUSH");
    }

    public override void UpdateState()
    {
        RotateTowardsTarget();

        // Keep pushing the path directly onto the player's toes
        Agent.SetDestination(Target.position);
        chargeTimer += Time.deltaTime;

        // If they close the distance into attack range, swing immediately!
        if (AI.sqrDistToTarget <= AI.attackRange * AI.attackRange)
        {
            if (AI.TrySwitchState<AttackState>())
                return;
        }

        // Safeguard: If they charge for too long without hitting the player, 
        // break out and rethink strategy
        if (chargeTimer >= maxChargeDuration)
        {
            AI.DetermineNextState();
        }
    }

    public override void ExitState()
    {
        // Reset NavMesh agent modifications back to inspector defaults
        Agent.speed = originalSpeed;
        Agent.acceleration = originalAcceleration;
        Agent.ResetPath();
    }
}
