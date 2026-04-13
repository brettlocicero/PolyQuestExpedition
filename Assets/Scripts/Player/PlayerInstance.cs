using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance instance;
    void Awake()
    {
        instance = this;
    }
}
