using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Melee Attack State")]
public class AttackStateSO : EnemyStateSO
{
    public override System.Type StateType => typeof(AttackState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new AttackState(ai, this);
    }

    public override bool CanBeSelected(EnemyAI ai)
    {
        return base.CanBeSelected(ai) &&
               ai.CanAttack() &&
               ai.sqrDistToTarget <= ai.attackRange * ai.attackRange;
    }
}

public class AttackState : EnemyBaseState
{
    private float attackTimer;
    private float activeSwingDuration = 0.6f; // Time spent executing the attack strike
    private bool standardSequenceComplete;

    public AttackState(EnemyAI ai, EnemyStateSO definition) : base(ai, definition) {}

    public override void EnterState()
    {
        attackTimer = 0f;
        standardSequenceComplete = false;
        
        // Halt NavMesh tracking instantly so they commit to the stance
        Agent.isStopped = true; 
        Agent.ResetPath();
        
        if (AI.anim)
            AI.anim.SetTrigger("Attack");
    }

    public override void UpdateState()
    {
        attackTimer += Time.deltaTime;

        // Phase 1: Active Swing Wind-up
        if (!standardSequenceComplete)
        {
            // Track the target subtly only during the first 30% of the strike
            if (attackTimer < activeSwingDuration * 0.3f)
            {
                RotateTowardsTarget();
            }

            // Once the physical strike animation timeframe ends, stamp the cooldown clock
            if (attackTimer >= activeSwingDuration)
            {
                AI.lastAttackTime = Time.time;
                standardSequenceComplete = true;
            }
        }
        else
        {
            // Phase 2: Post-Attack Recovery Window
            // The enemy stays completely still (Agent.isStopped is still true)
            
            float timeSpentRecovering = attackTimer - activeSwingDuration;
            
            if (timeSpentRecovering >= AI.attackRecoveryDuration)
            {
                // Recovery over! Safely evaluate where to move next
                AI.DetermineNextState();
            }
        }
    }

    public override void ExitState()
    {
        // Safety reset: ensure the NavMesh agent is allowed to move again upon leaving the state
        Agent.isStopped = false;
    }
}
