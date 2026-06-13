using System.Collections.Generic;
using UnityEngine;

public class RegionRoom : MonoBehaviour
{
    [Header("Bounds")]
    public Collider roomCollider;

    [Header("Connectors")]
    public List<Transform> connectors = new();

    [SerializeField, HideInInspector] Transform northConnector;
    [SerializeField, HideInInspector] Transform southConnector;
    [SerializeField, HideInInspector] Transform westConnector;
    [SerializeField, HideInInspector] Transform eastConnector;

    [Header("Doorways")]
    [SerializeField] GameObject doorwayPrefab;
    [SerializeField] GameObject doorwayFillerPrefab;

    readonly List<Transform> usedConnectors = new();
    readonly List<Transform> spawnedDoorwayConnectors = new();
    readonly Dictionary<Transform, GameObject> spawnedDoorwayFillers = new();
    bool initialized;

    public List<Transform> GetConnectors()
    {
        AddLegacyConnectors();
        return connectors;
    }

    public void InitRoom(Transform usedConnector = null)
    {
        AddLegacyConnectors();
        AddUsedConnector(usedConnector);

        if (initialized)
            return;

        initialized = true;
        SpawnDoorways();
    }

    public void AddUsedConnector(Transform connector)
    {
        if (connector == null || usedConnectors.Contains(connector))
            return;

        usedConnectors.Add(connector);

        if (initialized)
        {
            RemoveDoorwayFiller(connector);
            SpawnDoorway(connector);
        }
    }

    public void SpawnDoorways()
    {
        AddLegacyConnectors();

        for (int i = 0; i < usedConnectors.Count; i++)
        {
            SpawnDoorway(usedConnectors[i]);
        }

        for (int i = 0; i < connectors.Count; i++)
        {
            Transform connector = connectors[i];
            if (!usedConnectors.Contains(connector))
                SpawnDoorwayFiller(connector);
        }
    }

    void SpawnDoorway(Transform connector)
    {
        if (doorwayPrefab == null || connector == null || spawnedDoorwayConnectors.Contains(connector))
            return;

        Instantiate(doorwayPrefab, connector.position, connector.rotation, transform);
        spawnedDoorwayConnectors.Add(connector);
    }

    void SpawnDoorwayFiller(Transform connector)
    {
        if (doorwayFillerPrefab == null || connector == null || spawnedDoorwayFillers.ContainsKey(connector))
            return;

        GameObject filler = Instantiate(doorwayFillerPrefab, connector.position, connector.rotation, transform);
        spawnedDoorwayFillers.Add(connector, filler);
    }

    void RemoveDoorwayFiller(Transform connector)
    {
        if (connector == null || !spawnedDoorwayFillers.TryGetValue(connector, out GameObject filler))
            return;

        if (filler != null)
            Destroy(filler);

        spawnedDoorwayFillers.Remove(connector);
    }

    void OnValidate()
    {
        AddLegacyConnectors();
    }

    void AddLegacyConnectors()
    {
        AddLegacyConnector(northConnector);
        AddLegacyConnector(southConnector);
        AddLegacyConnector(westConnector);
        AddLegacyConnector(eastConnector);
    }

    void AddLegacyConnector(Transform connector)
    {
        if (connector == null)
            return;

        connectors ??= new List<Transform>();

        if (!connectors.Contains(connector))
            connectors.Add(connector);
    }
}
