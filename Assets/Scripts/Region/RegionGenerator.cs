using System.Collections.Generic;
using UnityEngine;

public class RegionGenerator : MonoBehaviour
{
    public static RegionGenerator instance;
    void Awake()
    {
        instance = this;
    }

    [Header("References")]
    [SerializeField] GameObject hubObjects;

    [Header("Noise")]
    [SerializeField, Min(0f)] float roomSpacingNoise = 0f;
    [SerializeField, Min(0f)] float lateralNoise = 0f;
    [SerializeField, Min(0f)] float verticalNoise = 0f;

    readonly List<RegionRoom> spawnedRooms = new();
    readonly List<GameObject> spawnedHallways = new();
    readonly List<OpenConnector> openConnectors = new();

    struct OpenConnector
    {
        public RegionRoom Room;
        public Transform Connector;
    }

    public void EnterRegion(RegionSO region)
    {
        Generate(region);
        region.ApplyVFX();

        hubObjects.SetActive(false);
    }

    [ContextMenu("Generate Region")]
    public void Generate(RegionSO region)
    {
        ClearGeneratedRooms();

        if (region == null)
        {
            Debug.LogWarning("Cannot generate a region without a RegionSO.", this);
            return;
        }

        if (region.startingRoomPrefab == null)
        {
            Debug.LogWarning($"{region.name} does not have a starting room prefab assigned.", region);
            return;
        }

        RegionRoom startRoom = Instantiate(region.startingRoomPrefab, transform.position, transform.rotation, transform);
        AcceptRoom(startRoom, null);

        int attempts = 0;
        while (spawnedRooms.Count < region.roomCount && openConnectors.Count > 0 && attempts < region.maxPlacementAttempts)
        {
            attempts++;

            int openConnectorIndex = Random.Range(0, openConnectors.Count);
            OpenConnector targetConnector = openConnectors[openConnectorIndex];

            RegionRoom prefab = GetRandomRoomPrefab(region);
            if (prefab == null)
                break;

            RegionRoom candidateRoom = Instantiate(prefab, targetConnector.Connector.position, Quaternion.identity, transform);
            Transform candidateConnector = GetRandomConnector(candidateRoom);

            if (candidateConnector == null)
            {
                DestroyRoom(candidateRoom);
                openConnectors.RemoveAt(openConnectorIndex);
                continue;
            }

            Vector3 connectionOffset = GetConnectionOffset(region, targetConnector);
            AlignConnector(candidateRoom, candidateConnector, targetConnector, connectionOffset);

            Physics.SyncTransforms();

            if (IntersectsSpawnedRoom(region, candidateRoom))
            {
                DestroyRoom(candidateRoom);
                openConnectors.RemoveAt(openConnectorIndex);
                continue;
            }

            openConnectors.RemoveAt(openConnectorIndex);
            SpawnHallway(region, targetConnector.Connector, candidateConnector);
            targetConnector.Room.AddUsedConnector(targetConnector.Connector);
            AcceptRoom(candidateRoom, candidateConnector);
        }

        if (attempts == region.maxPlacementAttempts)
            Debug.LogWarning("Reached maximum tries");
    }

    RegionRoom GetRandomRoomPrefab(RegionSO region)
    {
        if (region.roomPrefabs == null || region.roomPrefabs.Length == 0)
            return region.startingRoomPrefab;

        return region.roomPrefabs[Random.Range(0, region.roomPrefabs.Length)];
    }

    void AcceptRoom(RegionRoom room, Transform usedConnector)
    {
        spawnedRooms.Add(room);
        List<Transform> connectors = room.GetConnectors();

        for (int i = 0; i < connectors.Count; i++)
        {
            AddOpenConnector(room, connectors[i], usedConnector);
        }

        room.InitRoom(usedConnector);
    }

    void AddOpenConnector(RegionRoom room, Transform connector, Transform usedConnector)
    {
        if (connector == null || connector == usedConnector)
            return;

        openConnectors.Add(new OpenConnector
        {
            Room = room,
            Connector = connector
        });
    }

    void AlignConnector(RegionRoom room, Transform roomConnector, OpenConnector targetConnector, Vector3 connectionOffset)
    {
        Quaternion connectorLocalRotation = Quaternion.Inverse(room.transform.rotation) * roomConnector.rotation;
        Quaternion targetRotation = targetConnector.Connector.rotation * Quaternion.Euler(0f, 180f, 0f) * Quaternion.Inverse(connectorLocalRotation);
        Vector3 targetPosition = targetConnector.Connector.position + connectionOffset;

        room.transform.rotation = targetRotation;
        room.transform.position += targetPosition - roomConnector.position;
    }

    void SpawnHallway(RegionSO region, Transform startConnector, Transform endConnector)
    {
        Vector3 start = startConnector.position;
        Vector3 end = endConnector.position;
        Vector3 connectorDelta = end - start;
        float hallwayLength = connectorDelta.magnitude;

        if (region.hallwayPrefab == null || region.hallwayPrefabLength <= Mathf.Epsilon || hallwayLength <= Mathf.Epsilon)
            return;

        GameObject hallway = Instantiate(
            region.hallwayPrefab,
            start + connectorDelta * 0.5f,
            Quaternion.LookRotation(connectorDelta.normalized, Vector3.up),
            transform
        );

        Vector3 hallwayScale = hallway.transform.localScale;
        hallwayScale.z *= hallwayLength / region.hallwayPrefabLength;
        hallway.transform.localScale = hallwayScale;

        spawnedHallways.Add(hallway);
    }

    Vector3 GetConnectionOffset(RegionSO region, OpenConnector targetConnector)
    {
        float spacing = Mathf.Max(0f, region.roomSpacing + Random.Range(-roomSpacingNoise, roomSpacingNoise));
        float lateral = Random.Range(-lateralNoise, lateralNoise);
        float vertical = Random.Range(-verticalNoise, verticalNoise);

        if (targetConnector.Connector != null)
        {
            return targetConnector.Connector.forward * spacing
                + targetConnector.Connector.right * lateral
                + Vector3.up * vertical;
        }

        return Vector3.forward * spacing + Vector3.right * lateral + Vector3.up * vertical;
    }

    bool IntersectsSpawnedRoom(RegionSO region, RegionRoom candidateRoom)
    {
        if (candidateRoom.roomCollider == null)
        {
            Debug.LogWarning($"{candidateRoom.name} does not have a room collider assigned.", candidateRoom);
            return true;
        }

        Bounds candidateBounds = GetTestBounds(region, candidateRoom.roomCollider);

        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            RegionRoom spawnedRoom = spawnedRooms[i];
            if (spawnedRoom == null || spawnedRoom.roomCollider == null)
                continue;

            if (candidateBounds.Intersects(GetTestBounds(region, spawnedRoom.roomCollider)))
                return true;
        }

        return false;
    }

    Bounds GetTestBounds(RegionSO region, Collider roomCollider)
    {
        Bounds bounds = roomCollider.bounds;

        if (region.colliderShrink > 0f)
        {
            float shrink = Mathf.Min(region.colliderShrink, bounds.size.x * 0.49f, bounds.size.y * 0.49f, bounds.size.z * 0.49f);
            bounds.Expand(-shrink * 2f);
        }

        return bounds;
    }

    Transform GetRandomConnector(RegionRoom room)
    {
        List<Transform> connectors = room.GetConnectors();

        if (connectors == null || connectors.Count == 0)
            return null;

        int validConnectorCount = 0;
        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i] != null)
                validConnectorCount++;
        }

        if (validConnectorCount == 0)
            return null;

        int selectedConnectorIndex = Random.Range(0, validConnectorCount);
        for (int i = 0; i < connectors.Count; i++)
        {
            Transform connector = connectors[i];
            if (connector == null)
                continue;

            if (selectedConnectorIndex == 0)
                return connector;

            selectedConnectorIndex--;
        }

        return null;
    }

    void ClearGeneratedRooms()
    {
        for (int i = spawnedHallways.Count - 1; i >= 0; i--)
        {
            if (spawnedHallways[i] != null)
                DestroyObject(spawnedHallways[i]);
        }

        for (int i = spawnedRooms.Count - 1; i >= 0; i--)
        {
            if (spawnedRooms[i] != null)
                DestroyRoom(spawnedRooms[i]);
        }

        spawnedRooms.Clear();
        spawnedHallways.Clear();
        openConnectors.Clear();
    }

    void DestroyRoom(RegionRoom room)
    {
        DestroyObject(room.gameObject);
    }

    void DestroyObject(GameObject obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
}
