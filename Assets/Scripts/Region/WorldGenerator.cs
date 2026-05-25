using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int worldSize = 128;
    [SerializeField] float cellSize = 10f;

    [Header("Regions")]
    [SerializeField] int regionPointCount = 12;

    [Header("Border Noise")]
    [SerializeField] float borderNoiseScale = 0.05f;
    [SerializeField] float borderNoiseStrength = 15f;

    [Header("World Shape")]
    [SerializeField] float worldRadius = 0.9f;
    [SerializeField] float radiusNoiseScale = 0.02f;
    [SerializeField] float radiusNoiseStrength = 0.12f;

    [Header("Generation")]
    [SerializeField] int seed = 0;

    [Header("Debug")]
    [SerializeField] Renderer debugRenderer;
    [SerializeField] Color voidColor = Color.black;

    [Header("Regions")]
    [SerializeField] RegionTypeSO[] regionTypes;

    RegionTypeSO[,] regions;
    Texture2D texture;

    Vector2[] regionPoints;
    RegionTypeSO[] pointRegions;

    List<Vector3> spawnedPOIPositions = new();

    float halfWorld;

    void Start()
    {
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        regions = new RegionTypeSO[worldSize, worldSize];

        texture = new Texture2D(worldSize, worldSize);
        texture.filterMode = FilterMode.Point;

        halfWorld = worldSize * 0.5f;

        GenerateRegionPoints();

        System.Random prng = new System.Random(seed);

        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                float centeredX = (x - halfWorld) / halfWorld;
                float centeredY = (y - halfWorld) / halfWorld;

                float distanceFromCenter = Mathf.Sqrt(centeredX * centeredX + centeredY * centeredY);

                float radiusNoise = Mathf.PerlinNoise(
                    (x + offsetX) * radiusNoiseScale,
                    (y + offsetY) * radiusNoiseScale
                );

                float noisyRadius = worldRadius + ((radiusNoise - 0.5f) * radiusNoiseStrength);

                if (distanceFromCenter > noisyRadius)
                {
                    regions[x, y] = null;
                    texture.SetPixel(x, y, voidColor);
                    continue;
                }

                float noiseX = Mathf.PerlinNoise(
                    (x + offsetX) * borderNoiseScale,
                    (y + offsetY) * borderNoiseScale
                );

                float noiseY = Mathf.PerlinNoise(
                    (x + offsetX + 5000) * borderNoiseScale,
                    (y + offsetY + 5000) * borderNoiseScale
                );

                Vector2 distortedPos = new Vector2(
                    x + (noiseX - 0.5f) * borderNoiseStrength,
                    y + (noiseY - 0.5f) * borderNoiseStrength
                );

                RegionTypeSO region = GetNearestRegion(distortedPos);

                regions[x, y] = region;

                texture.SetPixel(x, y, region.GetColor());
            }
        }

        texture.Apply();

        if (debugRenderer != null)
            debugRenderer.material.mainTexture = texture;

        GeneratePOIs();
    }

    void GenerateRegionPoints()
    {
        regionPoints = new Vector2[regionPointCount];
        pointRegions = new RegionTypeSO[regionPointCount];

        System.Random prng = new System.Random(seed);

        float radius = halfWorld * worldRadius * 0.85f;

        for (int i = 0; i < regionPointCount; i++)
        {
            Vector2 point;

            int safety = 0;

            do
            {
                point = new Vector2(
                    prng.Next(0, worldSize),
                    prng.Next(0, worldSize)
                );

                safety++;
            }
            while (
                Vector2.Distance(point, Vector2.one * halfWorld) > radius &&
                safety < 100
            );

            regionPoints[i] = point;

            pointRegions[i] = regionTypes[i % regionTypes.Length];
        }
    }

    void GeneratePOIs()
    {
        int attempts = worldSize * 4;

        for (int i = 0; i < attempts; i++)
        {
            int x = Random.Range(0, worldSize);
            int y = Random.Range(0, worldSize);

            RegionTypeSO region = regions[x, y];

            if (region == null)
                continue;

            RegionPointOfInterestSO poi = region.GetRandomPOI();

            if (poi == null)
                continue;

            Vector3 worldPos = GridToWorldPosition(x, y);

            if (IsTooCloseToOtherPOIs(worldPos, 15f))
                continue;

            spawnedPOIPositions.Add(worldPos);

            Instantiate(poi.prefab, worldPos, Quaternion.identity);
        }
    }

    bool IsTooCloseToOtherPOIs(Vector3 position, float minDistance)
    {
        for (int i = 0; i < spawnedPOIPositions.Count; i++)
        {
            if (Vector3.Distance(position, spawnedPOIPositions[i]) < minDistance)
                return true;
        }

        return false;
    }

    RegionTypeSO GetNearestRegion(Vector2 pos)
    {
        float closestDistance = float.MaxValue;
        RegionTypeSO closestRegion = null;

        for (int i = 0; i < regionPoints.Length; i++)
        {
            float distance = Vector2.Distance(pos, regionPoints[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRegion = pointRegions[i];
            }
        }

        return closestRegion;
    }

    Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(
            (x - halfWorld) * cellSize,
            0,
            (y - halfWorld) * cellSize
        );
    }

    public RegionTypeSO GetRegionAtPosition(Vector3 worldPos)
    {
        int x = Mathf.Clamp(
            Mathf.FloorToInt((worldPos.x / cellSize) + halfWorld),
            0,
            worldSize - 1
        );

        int y = Mathf.Clamp(
            Mathf.FloorToInt((worldPos.z / cellSize) + halfWorld),
            0,
            worldSize - 1
        );

        return regions[x, y];
    }
}