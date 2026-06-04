using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] DungeonRoom startingRoomPrefab;
    [SerializeField] DungeonRoom[] roomPrefabs;
    [SerializeField, Min(1)] int roomCount = 10;

    [Header("Hallways")]
    [SerializeField] GameObject hallwayPrefab;
    [SerializeField, Min(0.001f)] float hallwayPrefabLength = 1f;

    [Header("Placement")]
    [SerializeField, Min(1)] int maxPlacementAttempts = 100;
    [SerializeField, Min(0f)] float colliderShrink = 0.05f;
    [SerializeField] float roomSpacing = 5f;

    [Header("Noise")]
    [SerializeField, Min(0f)] float roomSpacingNoise = 0f;
    [SerializeField, Min(0f)] float lateralNoise = 0f;
    [SerializeField, Min(0f)] float verticalNoise = 0f;

    readonly List<DungeonRoom> spawnedRooms = new List<DungeonRoom>();
    readonly List<GameObject> spawnedHallways = new List<GameObject>();
    readonly List<OpenConnector> openConnectors = new List<OpenConnector>();

    enum ConnectorDirection
    {
        North,
        South,
        West,
        East
    }

    struct OpenConnector
    {
        public DungeonRoom Room;
        public Transform Connector;
        public ConnectorDirection Direction;
    }

    void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearGeneratedRooms();
        DungeonRoom startRoom = Instantiate(startingRoomPrefab, transform.position, transform.rotation, transform);
        AcceptRoom(startRoom, null);

        int attempts = 0;
        while (spawnedRooms.Count < roomCount && openConnectors.Count > 0 && attempts < maxPlacementAttempts)
        {
            print($"attempt {attempts}");
            attempts++;

            int openConnectorIndex = Random.Range(0, openConnectors.Count);
            OpenConnector targetConnector = openConnectors[openConnectorIndex];

            DungeonRoom prefab = GetRandomRoomPrefab();
            if (prefab == null)
                break;

            DungeonRoom candidateRoom = Instantiate(prefab, targetConnector.Connector.position, Quaternion.identity, transform);
            Transform candidateConnector = GetConnector(candidateRoom, GetOppositeDirection(targetConnector.Direction));

            if (candidateConnector == null)
            {
                DestroyRoom(candidateRoom);
                openConnectors.RemoveAt(openConnectorIndex);
                continue;
            }

            Vector3 connectionOffset = GetConnectionOffset(targetConnector);
            AlignConnector(candidateRoom, candidateConnector, targetConnector, connectionOffset);

            Physics.SyncTransforms();

            if (IntersectsSpawnedRoom(candidateRoom))
            {
                DestroyRoom(candidateRoom);
                openConnectors.RemoveAt(openConnectorIndex);
                continue;
            }

            openConnectors.RemoveAt(openConnectorIndex);
            SpawnHallway(targetConnector.Connector, candidateConnector);
            AcceptRoom(candidateRoom, candidateConnector);
        }

        if (attempts == maxPlacementAttempts)
            Debug.LogWarning("Reached maximum tries");
    }

    DungeonRoom GetRandomRoomPrefab()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
            return startingRoomPrefab;

        return roomPrefabs[Random.Range(0, roomPrefabs.Length)];
    }

    void AcceptRoom(DungeonRoom room, Transform usedConnector)
    {
        spawnedRooms.Add(room);
        AddOpenConnector(room, room.northConnector, ConnectorDirection.North, usedConnector);
        AddOpenConnector(room, room.southConnector, ConnectorDirection.South, usedConnector);
        AddOpenConnector(room, room.westConnector, ConnectorDirection.West, usedConnector);
        AddOpenConnector(room, room.eastConnector, ConnectorDirection.East, usedConnector);
    }

    void AddOpenConnector(DungeonRoom room, Transform connector, ConnectorDirection direction, Transform usedConnector)
    {
        if (connector == null || connector == usedConnector)
            return;

        openConnectors.Add(new OpenConnector
        {
            Room = room,
            Connector = connector,
            Direction = direction
        });
    }

    void AlignConnector(DungeonRoom room, Transform roomConnector, OpenConnector targetConnector, Vector3 connectionOffset)
    {
        Quaternion connectorLocalRotation = Quaternion.Inverse(room.transform.rotation) * roomConnector.rotation;
        Quaternion targetRotation = targetConnector.Connector.rotation * Quaternion.Euler(0f, 180f, 0f) * Quaternion.Inverse(connectorLocalRotation);
        Vector3 targetPosition = targetConnector.Connector.position + connectionOffset;

        room.transform.rotation = targetRotation;
        room.transform.position += targetPosition - roomConnector.position;
    }

    void SpawnHallway(Transform startConnector, Transform endConnector)
    {
        if (hallwayPrefab == null || startConnector == null || endConnector == null)
            return;

        Vector3 start = startConnector.position;
        Vector3 end = endConnector.position;
        Vector3 connectorDelta = end - start;
        float hallwayLength = connectorDelta.magnitude;

        if (hallwayLength <= Mathf.Epsilon)
            return;

        GameObject hallway = Instantiate(
            hallwayPrefab,
            start + connectorDelta * 0.5f,
            Quaternion.LookRotation(connectorDelta.normalized, Vector3.up),
            transform
        );

        Vector3 hallwayScale = hallway.transform.localScale;
        hallwayScale.z *= hallwayLength / hallwayPrefabLength;
        hallway.transform.localScale = hallwayScale;

        spawnedHallways.Add(hallway);
    }

    Vector3 GetConnectionOffset(OpenConnector targetConnector)
    {
        float spacing = Mathf.Max(0f, roomSpacing + Random.Range(-roomSpacingNoise, roomSpacingNoise));
        float lateral = Random.Range(-lateralNoise, lateralNoise);
        float vertical = Random.Range(-verticalNoise, verticalNoise);

        if (targetConnector.Connector != null)
        {
            return targetConnector.Connector.forward * spacing
                + targetConnector.Connector.right * lateral
                + Vector3.up * vertical;
        }

        return targetConnector.Direction switch
        {
            ConnectorDirection.North => Vector3.forward * spacing + Vector3.right * lateral + Vector3.up * vertical,
            ConnectorDirection.South => Vector3.back * spacing + Vector3.left * lateral + Vector3.up * vertical,
            ConnectorDirection.West => Vector3.left * spacing + Vector3.back * lateral + Vector3.up * vertical,
            ConnectorDirection.East => Vector3.right * spacing + Vector3.forward * lateral + Vector3.up * vertical,
            _ => Vector3.zero,
        };
    }

    bool IntersectsSpawnedRoom(DungeonRoom candidateRoom)
    {
        if (candidateRoom.roomCollider == null)
        {
            Debug.LogWarning($"{candidateRoom.name} does not have a room collider assigned.", candidateRoom);
            return true;
        }

        Bounds candidateBounds = GetTestBounds(candidateRoom.roomCollider);

        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            DungeonRoom spawnedRoom = spawnedRooms[i];
            if (spawnedRoom == null || spawnedRoom.roomCollider == null)
                continue;

            if (candidateBounds.Intersects(GetTestBounds(spawnedRoom.roomCollider)))
                return true;
        }

        return false;
    }

    Bounds GetTestBounds(Collider roomCollider)
    {
        Bounds bounds = roomCollider.bounds;

        if (colliderShrink > 0f)
        {
            float shrink = Mathf.Min(colliderShrink, bounds.size.x * 0.49f, bounds.size.y * 0.49f, bounds.size.z * 0.49f);
            bounds.Expand(-shrink * 2f);
        }

        return bounds;
    }

    Transform GetConnector(DungeonRoom room, ConnectorDirection direction)
    {
        return direction switch
        {
            ConnectorDirection.North => room.northConnector,
            ConnectorDirection.South => room.southConnector,
            ConnectorDirection.West => room.westConnector,
            ConnectorDirection.East => room.eastConnector,
            _ => null,
        };
    }

    ConnectorDirection GetOppositeDirection(ConnectorDirection direction)
    {
        return direction switch
        {
            ConnectorDirection.North => ConnectorDirection.South,
            ConnectorDirection.South => ConnectorDirection.North,
            ConnectorDirection.West => ConnectorDirection.East,
            ConnectorDirection.East => ConnectorDirection.West,
            _ => ConnectorDirection.North,
        };
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

    void DestroyRoom(DungeonRoom room)
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
