using UnityEngine;

/// <summary>
/// Mobile first-person movement controller using CharacterController for low-overhead kinematic movement.
/// Reads touch input from TouchInputManager and keeps movement and look decoupled.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MobilePlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TouchInputManager inputManager;
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.2f;
    [SerializeField] private float sprintSpeed = 3.4f;
    [SerializeField] private float gravity = -18f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 110f;
    [SerializeField] private float minPitch = -75f;
    [SerializeField] private float maxPitch = 75f;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 2.4f;
    [SerializeField] private LayerMask interactLayers;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;
    private float speedMultiplier = 1f;

    public float CurrentSpeedNormalized { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (inputManager == null)
        {
            inputManager = FindObjectOfType<TouchInputManager>();
        }
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInteraction();
    }

    public void SetMovementSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    private void HandleLook()
    {
        Vector2 look = inputManager != null ? inputManager.LookDelta : Vector2.zero;

        float yaw = look.x * lookSensitivity;
        float pitchDelta = look.y * lookSensitivity;

        transform.Rotate(0f, yaw, 0f);

        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = inputManager != null ? inputManager.MoveInput : Vector2.zero;
        bool sprintHeld = inputManager != null && inputManager.SprintHeld;

        float speed = (sprintHeld ? sprintSpeed : walkSpeed) * speedMultiplier;
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        CurrentSpeedNormalized = speed > 0.01f ? moveInput.magnitude : 0f;
    }

    private void HandleInteraction()
    {
        if (inputManager == null || !inputManager.InteractPressed)
        {
            return;
        }

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactRange, interactLayers))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
            }
        }
    }
}

public interface IInteractable
{
    void Interact(GameObject interactor);
}
