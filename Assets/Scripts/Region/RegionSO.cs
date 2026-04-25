using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName = "Unnamed Region";

    [Header("")]
    public RegionFloor[] floors;
}
