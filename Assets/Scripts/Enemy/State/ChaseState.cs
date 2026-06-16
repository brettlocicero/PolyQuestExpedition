using UnityEngine;

public class ChaseState : EnemyBaseState
{
    public ChaseState(EnemyAI ai) : base(ai) {}

    public override void EnterState() { }

    public override void UpdateState()
    {
        RotateTowardsTarget();

        bool isEngaged = AI.sqrDistToTarget <= AI.engageDistance * AI.engageDistance || AI.tookDamage;
        if (!isEngaged) return; // Acts like your original Idle state structure

        // Keep path updated to chase down target
        Agent.SetDestination(Target.position);

        // Transition to tactical movement if inside range threshold
        if (AI.sqrDistToTarget <= AI.combatThresholdRange * AI.combatThresholdRange)
        {
            AI.SwitchState(AI.StrafeState);
        }
    }

    public override void ExitState()
    {
        Agent.ResetPath();
    }
}