using UnityEngine;

public class StunnedState : EnemyBaseState
{
    public StunnedState(EnemyAI ai) : base(ai) {}

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