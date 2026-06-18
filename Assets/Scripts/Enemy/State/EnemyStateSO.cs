using System;
using UnityEngine;

public abstract class EnemyStateSO : ScriptableObject
{
    [SerializeField, Min(0f)] float selectionWeight = 1f;

    public float SelectionWeight => selectionWeight;
    public abstract Type StateType { get; }

    public abstract EnemyBaseState CreateState(EnemyAI ai);

    public virtual bool CanBeSelected(EnemyAI ai)
    {
        return selectionWeight > 0f;
    }
}
