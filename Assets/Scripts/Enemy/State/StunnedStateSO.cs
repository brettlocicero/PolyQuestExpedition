using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/States/Stunned State")]
public class StunnedStateSO : EnemyStateSO
{
    public override System.Type StateType => typeof(StunnedState);

    public override EnemyBaseState CreateState(EnemyAI ai)
    {
        return new StunnedState(ai, this);
    }
}

public class StunnedState : EnemyBaseState
{
    public StunnedState(EnemyAI ai, EnemyStateSO definition) : base(ai, definition) {}

    public override void EnterState()
    {
        Agent.isStopped = true;
        Agent.ResetPath(); // Halt tracking tracking paths instantly
    }

    public override void UpdateState()
    {
        // Don't move or do anything. The AI controller handles tracking the timer.
        // Once the timer hits 0, it calls DetermineNextState() automatically.
    }

    public override void ExitState()
    {
        Agent.isStopped = false;
    }
}
