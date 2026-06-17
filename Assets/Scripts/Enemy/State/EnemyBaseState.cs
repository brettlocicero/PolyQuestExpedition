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
        dir.y = 0f; // Keep the enemy flat on the ground plane so they don't tilt up/down

        // Prevent errors if the enemy is precisely on top of the player
        if (dir.sqrMagnitude < 0.01f) 
            return;

        // Calculate the look rotation
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        
        // Smoothly interpolate from current rotation to target rotation using Time.deltaTime
        AI.transform.rotation = Quaternion.Slerp(
            AI.transform.rotation, 
            targetRotation, 
            AI.rotationSpeed * Time.deltaTime
        );
    }
}