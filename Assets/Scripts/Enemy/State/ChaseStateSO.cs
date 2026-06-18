using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Chase State")]
public class ChaseStateSO : EnemyStateSO
{
    public override System.Type StateType => typeof(ChaseState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new ChaseState(ai, this);
    }
}

public class ChaseState : EnemyBaseState
{
    public ChaseState(EnemyAI ai, EnemyStateSO definition) : base(ai, definition) {}

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
            AI.DetermineNextState();
        }
    }

    public override void ExitState()
    {
        Agent.ResetPath();
    }
}
