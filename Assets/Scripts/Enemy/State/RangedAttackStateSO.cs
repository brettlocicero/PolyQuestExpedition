using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Ranged Attack State")]
public class RangedAttackStateSO : EnemyStateSO
{
    [SerializeField] float minRange = 3f;
    [SerializeField] float maxRange = 14f;
    [SerializeField] float windupDuration = 0.35f;
    [SerializeField] float recoveryDuration = 0.6f;
    [SerializeField] string attackTrigger = "Attack";
    [SerializeField] bool fireProjectileFromState = true;

    public float MinRange => minRange;
    public float MaxRange => maxRange;
    public float WindupDuration => windupDuration;
    public float RecoveryDuration => recoveryDuration;
    public string AttackTrigger => attackTrigger;
    public bool FireProjectileFromState => fireProjectileFromState;

    public override System.Type StateType => typeof(RangedAttackState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new RangedAttackState(ai, this);
    }

    public override bool CanBeSelected(EnemyAI ai)
    {
        float minRangeSqr = minRange * minRange;
        float maxRangeSqr = maxRange * maxRange;

        return base.CanBeSelected(ai) &&
               ai.CanAttack() &&
               ai.sqrDistToTarget >= minRangeSqr &&
               ai.sqrDistToTarget <= maxRangeSqr;
    }
}

public class RangedAttackState : EnemyBaseState
{
    private readonly RangedAttackStateSO settings;
    private KeyframeProjectile projectileLauncher;
    private float attackTimer;
    private bool projectileFired;

    public RangedAttackState(EnemyAI ai, RangedAttackStateSO definition) : base(ai, definition)
    {
        settings = definition;
        projectileLauncher = ai.GetComponent<KeyframeProjectile>();
    }

    public override void EnterState()
    {
        attackTimer = 0f;
        projectileFired = false;

        Agent.isStopped = true;
        Agent.ResetPath();

        if (AI.anim && !string.IsNullOrWhiteSpace(settings.AttackTrigger))
            AI.anim.SetTrigger(settings.AttackTrigger);
    }

    public override void UpdateState()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < settings.WindupDuration)
        {
            RotateTowardsTarget();
            return;
        }

        if (!projectileFired)
        {
            FireProjectile();
            AI.lastAttackTime = Time.time;
            projectileFired = true;
        }

        if (attackTimer >= settings.WindupDuration + settings.RecoveryDuration)
            AI.DetermineNextState();
    }

    public override void ExitState()
    {
        Agent.isStopped = false;
    }

    private void FireProjectile()
    {
        if (!settings.FireProjectileFromState || !projectileLauncher)
            return;

        projectileLauncher.SetDirection();
        projectileLauncher.LaunchProjectile();
    }
}
