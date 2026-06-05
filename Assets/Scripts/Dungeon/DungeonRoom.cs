using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [Header("Bounds")]
    public Collider roomCollider;

    [Header("Connectors")]
    public Transform northConnector;
    public Transform southConnector;
    public Transform westConnector;
    public Transform eastConnector;

    [Header("Doorways")]
    [SerializeField] GameObject doorwayPrefab;

    [Header("Props")]
    [SerializeField] GameObject[] propPrefabs;
    [SerializeField, Min(0)] int propSpawnCount = 0;
    [SerializeField, Min(1)] int propPlacementAttempts = 20;
    [SerializeField, Min(0f)] float connectorClearance = 1.5f;
    [SerializeField] float propYOffset = 0f;
    [SerializeField] bool randomizePropRotation = true;
    [SerializeField] bool randomizePropScale = false;
    [SerializeField] Vector2 propScaleRange = Vector2.one;

    readonly List<Transform> usedConnectors = new();
    readonly List<Transform> spawnedDoorwayConnectors = new();
    bool initialized;

    public void InitRoom(Transform usedConnector = null)
    {
        AddUsedConnector(usedConnector);

        if (initialized)
            return;

        initialized = true;
        SpawnDoorways();
        SpawnProps();
    }

    public void AddUsedConnector(Transform connector)
    {
        if (connector == null || usedConnectors.Contains(connector))
            return;

        usedConnectors.Add(connector);

        if (initialized)
            SpawnDoorway(connector);
    }

    public void SpawnDoorways()
    {
        for (int i = 0; i < usedConnectors.Count; i++)
            SpawnDoorway(usedConnectors[i]);
    }

    public void SpawnProps()
    {
        if (roomCollider == null || propPrefabs == null || propPrefabs.Length == 0 || propSpawnCount <= 0)
            return;

        Bounds bounds = roomCollider.bounds;
        for (int i = 0; i < propSpawnCount; i++)
        {
            GameObject propPrefab = GetRandomPropPrefab();
            if (propPrefab == null || !TryGetPropPosition(bounds, out Vector3 position))
                continue;

            Quaternion rotation = randomizePropRotation
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : Quaternion.identity;

            GameObject spawnedProp = Instantiate(propPrefab, position, rotation, transform);
            if (randomizePropScale)
            {
                float minScale = Mathf.Min(propScaleRange.x, propScaleRange.y);
                float maxScale = Mathf.Max(propScaleRange.x, propScaleRange.y);
                spawnedProp.transform.localScale *= Random.Range(minScale, maxScale);
            }
        }
    }

    void SpawnDoorway(Transform connector)
    {
        if (doorwayPrefab == null || connector == null || spawnedDoorwayConnectors.Contains(connector))
            return;

        Instantiate(doorwayPrefab, connector.position, connector.rotation, transform);
        spawnedDoorwayConnectors.Add(connector);
    }

    GameObject GetRandomPropPrefab()
    {
        for (int i = 0; i < propPrefabs.Length; i++)
        {
            GameObject prefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            if (prefab != null)
                return prefab;
        }

        return null;
    }

    bool TryGetPropPosition(Bounds bounds, out Vector3 position)
    {
        for (int i = 0; i < propPlacementAttempts; i++)
        {
            position = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y + propYOffset,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (IsInsideRoomFootprint(position) && IsClearOfConnectors(position))
                return true;
        }

        position = Vector3.zero;
        return false;
    }

    bool IsInsideRoomFootprint(Vector3 position)
    {
        Vector3 checkPosition = new(position.x, roomCollider.bounds.center.y, position.z);
        Vector3 closestPoint = roomCollider.ClosestPoint(checkPosition);
        return (closestPoint - checkPosition).sqrMagnitude <= 0.0001f;
    }

    bool IsClearOfConnectors(Vector3 position)
    {
        float clearanceSqr = connectorClearance * connectorClearance;
        return IsClearOfConnector(position, northConnector, clearanceSqr)
            && IsClearOfConnector(position, southConnector, clearanceSqr)
            && IsClearOfConnector(position, westConnector, clearanceSqr)
            && IsClearOfConnector(position, eastConnector, clearanceSqr);
    }

    bool IsClearOfConnector(Vector3 position, Transform connector, float clearanceSqr)
    {
        Vector2 propPosition = new(position.x, position.z);
        Vector2 connectorPosition = new(connector.position.x, connector.position.z);
        return (propPosition - connectorPosition).sqrMagnitude >= clearanceSqr;
    }
}
