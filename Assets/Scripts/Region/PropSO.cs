using UnityEngine;

[CreateAssetMenu(fileName = "PropSO", menuName = "Scriptable Objects/PropSO")]
public class PropSO : ScriptableObject
{
    public GameObject propObj;
    public int maxAmount = 100;

    [Header("Transformation")]
    public float yOffset;
    public bool randomizeScale;
    public Vector2 scaleRange;
    public bool randomizeRotation;
    public Vector2 rotationRange;
}
