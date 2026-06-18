using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Strafe State")]
public class StrafeStateSO : EnemyStateSO
{
    public override System.Type StateType => typeof(StrafeState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new StrafeState(ai, this);
    }
}

public class StrafeState : EnemyBaseState
{
    private float strafeDirection = 1f;
    private float decisionTimer;
    private float nextDecisionTime;

    public StrafeState(EnemyAI ai, EnemyStateSO definition) : base(ai, definition) {}

    public override void EnterState()
    {
        ChooseNewStrafeParameters();
    }

    public override void UpdateState()
    {
        RotateTowardsTarget();

        if (AI.sqrDistToTarget > AI.combatThresholdRange * AI.combatThresholdRange)
        {
            AI.TrySwitchState<ChaseState>();
            return;
        }

        // If inside range limits and timing permits, smash them!
        if (AI.CanAttack())
        {
            if (AI.TrySwitchSelectableState<AttackState>())
                return;

            if (AI.TrySwitchBestCombatState())
                return;
        }

        // Dynamic tactical repositioning updates
        decisionTimer += Time.deltaTime;
        if (decisionTimer >= nextDecisionTime)
        {
            ChooseNewStrafeParameters();
        }

        float currentDist = Mathf.Sqrt(AI.sqrDistToTarget);
        Vector3 rightDir = AI.transform.right * strafeDirection;
        Vector3 forwardDir = AI.transform.forward;

        // Calculate offset vector based on your original moveSpeed values
        Vector3 targetMoveDir = rightDir * 1.2f + forwardDir * (currentDist - AI.attackRange);
        Vector3 targetPos = AI.transform.position + targetMoveDir.normalized * 1.5f;

        Agent.SetDestination(targetPos);
    }

    private void ChooseNewStrafeParameters()
    {
        decisionTimer = 0f;
        nextDecisionTime = Random.Range(1f, 2.5f);
        
        // While circling, there's a chance the enemy gets aggressive and breaks into a charge
        if (Random.value <= 0.3f) // 30% chance to break out of a strafe loop into a charge
        {
            AI.TrySwitchState<ChargeState>();
            return;
        }

        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    public override void ExitState()
    {
        Agent.ResetPath();
    }
}
