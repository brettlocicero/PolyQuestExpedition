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

    [Header("Runtime")]
    [SerializeField] int health = 100;
    [SerializeField] int maxHealth = 100;

    [Header("VFX")]
    [SerializeField] Animator hitScreenAnim;
    
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        hitScreenAnim.SetTrigger("Hit");
    }
}
