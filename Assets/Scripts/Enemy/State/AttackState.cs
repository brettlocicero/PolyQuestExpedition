using UnityEngine;

public class AttackState : EnemyBaseState
{
    private float attackTimer;
    private float customAttackDuration = 0.85f; // Tweak to match weapon hit/swing timing

    public AttackState(EnemyAI ai) : base(ai) {}

    public override void EnterState()
    {
        attackTimer = 0f;
        Agent.isStopped = true; // Locks NavMesh so they commit physically to the stance
        
        if (AI.anim)
            AI.anim.SetTrigger("Attack");
    }

    public override void UpdateState()
    {
        attackTimer += Time.deltaTime;

        // Track target subtly during early wind up frames
        if (attackTimer < customAttackDuration * 0.3f)
        {
            RotateTowardsTarget();
        }

        if (attackTimer >= customAttackDuration)
        {
            AI.lastAttackTime = Time.time;
            AI.DetermineNextState(); // Evaluate where to slide safely next
        }
    }

    public override void ExitState()
    {
        Agent.isStopped = false;
    }
}