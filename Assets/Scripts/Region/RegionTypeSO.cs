using UnityEngine;

[CreateAssetMenu(menuName = "PolyQuest/Region/Region Type")]
public class RegionTypeSO : ScriptableObject
{
    public string regionName;
    public int poiBudget = 10;
    public float poiDensity = 1f;
    public Color debugColor;

    public float noiseMin;
    public float noiseMax;

    public RegionPointOfInterestSO[] pointsOfInterest;

    public bool Matches(float noise)
    {
        return noise >= noiseMin && noise < noiseMax;
    }

    public Color GetColor()
    {
        return debugColor;
    }

    public RegionPointOfInterestSO GetRandomPOI()
    {
        if (pointsOfInterest == null || pointsOfInterest.Length == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < pointsOfInterest.Length; i++)
        {
            totalWeight += pointsOfInterest[i].spawnWeight;
        }

        float roll = Random.value * totalWeight;

        for (int i = 0; i < pointsOfInterest.Length; i++)
        {
            roll -= pointsOfInterest[i].spawnWeight;

            if (roll <= 0)
                return pointsOfInterest[i];
        }

        return pointsOfInterest[0];
    }
}