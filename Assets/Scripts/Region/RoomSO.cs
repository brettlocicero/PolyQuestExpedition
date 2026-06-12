using UnityEngine;

[CreateAssetMenu(fileName = "RoomSO", menuName = "Scriptable Objects/RoomSO")]
public class RoomSO : ScriptableObject
{
    public GameObject roomObject;
    public GameObject mapObject;
    public float mapYPos;
}
