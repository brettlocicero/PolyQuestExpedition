using UnityEngine;

public class InteractionManager : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] float interactionRange = 4f;
	[SerializeField] LayerMask interactionLayerMask;

	Transform cam;

	void Start()
	{
		cam = Camera.main.transform;
	}

	void Update()
	{
		HandleInteract();
	}

    void FixedUpdate()
    {
        HandleHoverInteracts();
    }

    void HandleInteract()
	{
		if (InputManager.Actions.Player.Interact.IsPressed())
		{
			if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, interactionRange, interactionLayerMask))
			{
				if (hit.collider.TryGetComponent(out IInteractable interactable))
				{
					interactable.Interact();
					Debug.Log($"Interacted with {hit.collider.gameObject.name}...");
				}
			}
		}
	}

	void HandleHoverInteracts()
	{
		if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, interactionRange, interactionLayerMask))
		{
			if (hit.collider.TryGetComponent(out IInteractable interactable))
			{
				interactable.Hover("Press 'E' to interact with " + hit.collider.gameObject.name);
			}
		}

		else
		{
			// PopupManager.instance.HidePopup();
		}
	}
}
