using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] RegionSO region;

    [Header("Scene Layout")]
    [SerializeField] float nodeSpacingX = 4f;
    [SerializeField] float nodeSpacingZ = 6f;
    [SerializeField] float spacingNoise = 1.2f;
    
    [Header("Generation Settings")]
    [Range(0.1f, 1f)] 
    [SerializeField] float nodeSpawnChance = 0.75f; 
    
    [Tooltip("Maximum columns a path can drift horizontally per level. 1 is highly recommended for Slay the Spire maps.")]
    [SerializeField] int maxColumnDelta = 1; 

    [Header("VFX / Visuals")]
    [SerializeField] LineRenderer linePrefab; 

    void Start()
    {
        GenerateMap(region);
    }

    public void GenerateMap(RegionSO region)
    {
        List<List<MapNode>> mapLayers = new();

        for (int level = 0; level < region.mapLength; level++)
        {
            List<MapNode> currentLayer = new();
            float zPos = level * nodeSpacingZ;

            float progress = (float)level / (region.mapLength - 1);
            float curveWidthFactor = Mathf.Sin(progress * Mathf.PI); 
            
            int maxNodesInThisLevel = Mathf.RoundToInt(Mathf.Lerp(1f, region.maxNodesPerLevel, curveWidthFactor));
            maxNodesInThisLevel = Mathf.Max(1, maxNodesInThisLevel); 

            for (int nodeIndex = 0; nodeIndex < maxNodesInThisLevel; nodeIndex++)
            {
                bool isAnchorPoint = level == 0 || level == region.mapLength - 1;
                if (!isAnchorPoint && Random.value > nodeSpawnChance && currentLayer.Count > 0) 
                    continue;

                float totalWidth = (maxNodesInThisLevel - 1) * nodeSpacingX;
                float xPos = (nodeIndex * nodeSpacingX) - (totalWidth / 2f);

                if (maxNodesInThisLevel == 1) xPos = 0f;

                RoomSO room = region.GetRoomFromMapLevel(level);
                
                float noiseX = Random.Range(-spacingNoise, spacingNoise);
                float noiseZ = Random.Range(-spacingNoise, spacingNoise);
                
                if (isAnchorPoint) { noiseX = 0f; noiseZ = 0f; }

                Vector3 pos = new(xPos + noiseX, room.mapYPos, zPos + noiseZ);

                MapNode node = new(level, currentLayer.Count, nodeIndex, pos, room);
                currentLayer.Add(node);
            }
            mapLayers.Add(currentLayer);
        }

        for (int level = 0; level < mapLayers.Count - 1; level++)
        {
            List<MapNode> currentLayer = mapLayers[level];
            List<MapNode> nextLayer = mapLayers[level + 1];

            int maxConnectedIndexNextLayer = 0;

            for (int i = 0; i < currentLayer.Count; i++)
            {
                MapNode currentNode = currentLayer[i];

                int minValidIndex = -1;
                int maxValidIndex = -1;

                for (int k = 0; k < nextLayer.Count; k++)
                {
                    if (Mathf.Abs(currentNode.gridColumn - nextLayer[k].gridColumn) <= maxColumnDelta)
                    {
                        if (minValidIndex == -1) minValidIndex = k;
                        maxValidIndex = k;
                    }
                }

                if (minValidIndex == -1)
                {
                    int closestIndex = 0;
                    int minDelta = int.MaxValue;
                    for (int k = 0; k < nextLayer.Count; k++)
                    {
                        int delta = Mathf.Abs(currentNode.gridColumn - nextLayer[k].gridColumn);
                        if (delta < minDelta)
                        {
                            minDelta = delta;
                            closestIndex = k;
                        }
                    }
                    minValidIndex = closestIndex;
                    maxValidIndex = closestIndex;
                }

                minValidIndex = Mathf.Max(minValidIndex, maxConnectedIndexNextLayer);
                if (minValidIndex > maxValidIndex)
                {
                    minValidIndex = maxValidIndex;
                }

                float ratio = (float)i / Mathf.Max(1, currentLayer.Count - 1);
                int targetIndex = Mathf.RoundToInt(Mathf.Lerp(minValidIndex, maxValidIndex, ratio));
                targetIndex = Mathf.Clamp(targetIndex, minValidIndex, maxValidIndex);

                ConnectNodes(currentNode, nextLayer[targetIndex]);
                maxConnectedIndexNextLayer = targetIndex; 

                int rightNeighbor = targetIndex + 1;
                if (Random.value < 0.3f && rightNeighbor <= maxValidIndex)
                {
                    ConnectNodes(currentNode, nextLayer[rightNeighbor]);
                    maxConnectedIndexNextLayer = rightNeighbor;
                }
            }

            for (int j = 0; j < nextLayer.Count; j++)
            {
                MapNode nextNode = nextLayer[j];
                if (nextNode.incomingConnections.Count == 0)
                {
                    List<MapNode> validParents = new();
                    foreach (MapNode potentialParent in currentLayer)
                    {
                        if (Mathf.Abs(potentialParent.gridColumn - nextNode.gridColumn) <= maxColumnDelta)
                        {
                            validParents.Add(potentialParent);
                        }
                    }

                    if (validParents.Count > 0)
                    {
                        float ratio = (float)j / Mathf.Max(1, nextLayer.Count - 1);
                        int parentChoice = Mathf.RoundToInt(ratio * (validParents.Count - 1));
                        ConnectNodes(validParents[parentChoice], nextNode);
                    }
                    else
                    {
                        MapNode closestParent = currentLayer[0];
                        float minDst = Vector3.Distance(nextNode.position, closestParent.position);
                        foreach (MapNode p in currentLayer)
                        {
                            float dst = Vector3.Distance(nextNode.position, p.position);
                            if (dst < minDst)
                            {
                                minDst = dst;
                                closestParent = p;
                            }
                        }
                        
                        ConnectNodes(closestParent, nextNode);
                    }
                }
            }
        }

        foreach (List<MapNode> layer in mapLayers)
        {
            foreach (MapNode node in layer)
            {
                if (node.roomType.mapObject != null)
                {
                    Instantiate(node.roomType.mapObject, node.position, Quaternion.identity, transform);
                }
                
                foreach (MapNode nextNode in node.outgoingConnections)
                {
                    if (linePrefab != null)
                    {
                        LineRenderer line = Instantiate(linePrefab, transform);
                        line.positionCount = 2;
                        line.SetPosition(0, node.position);
                        line.SetPosition(1, nextNode.position);
                    }
                }
            }
        }
    }

    private void ConnectNodes(MapNode from, MapNode to)
    {
        if (!from.outgoingConnections.Contains(to))
        {
            from.outgoingConnections.Add(to);
            to.incomingConnections.Add(from);
        }
    }
}