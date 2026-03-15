using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] CharacterController playerCC;

    float currentMovement = 0f;
    float velocity = 0f;

    void Update()
    {
        HandleAnimation();
    }

    void HandleAnimation()
    {
        float targetMovement = InputManager.Actions.Player.Move.ReadValue<Vector2>().magnitude;
        if (!playerCC.isGrounded) targetMovement = 0;

        currentMovement = Mathf.SmoothDamp(currentMovement, targetMovement, ref velocity, 0.1f);
        animator.SetFloat("Movement", currentMovement);
    }
}
