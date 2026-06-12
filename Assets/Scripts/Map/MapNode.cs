using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public int level;
    public int index; 
    public int gridColumn; // Structural column slot
    public Vector3 position;
    public RoomSO roomType;
    
    public List<MapNode> outgoingConnections = new List<MapNode>();
    public List<MapNode> incomingConnections = new List<MapNode>(); 

    public MapNode(int level, int index, int gridColumn, Vector3 position, RoomSO roomType)
    {
        this.level = level;
        this.index = index;
        this.gridColumn = gridColumn;
        this.position = position;
        this.roomType = roomType;
    }
}