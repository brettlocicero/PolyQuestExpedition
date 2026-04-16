using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(CharacterController))]
public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance instance;
    void Awake()
    {
        instance = this;
    }
    
    PlayerController playerController;
    CharacterController cc;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        cc = GetComponent<CharacterController>();
    }
    
    public void RepositionPlayer(Vector3 pos) 
    {
        cc.enabled = false;
        transform.SetPositionAndRotation(pos, transform.rotation);
        cc.enabled = true;
    }
}
