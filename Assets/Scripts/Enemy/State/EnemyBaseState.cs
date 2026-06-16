using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBaseState
{
    protected EnemyAI AI;
    protected NavMeshAgent Agent;
    protected Transform Target;

    public EnemyBaseState(EnemyAI aiController)
    {
        this.AI = aiController;
        this.Agent = aiController.Agent;
        this.Target = aiController.target;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    protected void RotateTowardsTarget()
    {
        Vector3 dir = Target.position - AI.transform.position;
        dir.y = 0f;

        if (dir == Vector3.zero && !AI.alwaysLookAtPlayer)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, targetRotation, AI.rotationSpeed * Time.deltaTime);
    }
}