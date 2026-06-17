using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] int souls = 0;

    [Header("VFX")]
    [SerializeField] Animator hitScreenAnim;
    [SerializeField] Animator roomTransitionAnim;
    [SerializeField] Transform healthBarTransform;
    
    PlayerController playerController;
    CharacterController cc;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        cc = GetComponent<CharacterController>();
        health = maxHealth;
        UpdateHealthBarUI();
    }
    
    public PlayerController GetPlayerController() 
    {
        return playerController;
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        hitScreenAnim.SetTrigger("Hit");
        UpdateHealthBarUI();
    }

    public Vector3 GetPlayerVelocity()
    {
        return playerController.GetPlayerVelocity();
    }

    public void PlayRoomTransitionAnimation()
    {
        roomTransitionAnim.SetTrigger("RoomTransition");
    }

    public void RepositionPlayer(Vector3 pos) 
    {
        cc.enabled = false;
        transform.SetPositionAndRotation(pos, transform.rotation);
        cc.enabled = true;
    }

    public void RepositionPlayer(Vector3 pos, Quaternion rot)
    {
        cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        cc.enabled = true;
    }

    public void AddSouls(int souls)
    {
        this.souls += souls;
    }

    void UpdateHealthBarUI()
    {
        float xScale = health / (float)maxHealth;
        healthBarTransform.localScale = new Vector3(xScale, 1f, 1f);
    }
}
