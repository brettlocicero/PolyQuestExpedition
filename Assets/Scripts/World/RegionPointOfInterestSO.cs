using UnityEngine;

[CreateAssetMenu(menuName = "PolyQuest/Region/Point Of Interest")]
public class RegionPointOfInterestSO : ScriptableObject
{
    public string poiName;
    public GameObject prefab;
    public float spawnWeight = 1f;
    public int minDistanceFromOtherPOIs = 20;
    public int maxPerRegion = 5;
    public Vector2 randomScaleRange;
    public bool requiresFlatGround;
}