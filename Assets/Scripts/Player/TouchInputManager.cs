using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Collects mobile touch input in a lightweight, reusable way.
/// Left half of the screen controls movement, right half controls camera look.
/// UI buttons can call the public button methods directly.
/// </summary>
public class TouchInputManager : MonoBehaviour
{
    [Header("Touch Areas")]
    [SerializeField] private float deadZone = 0.05f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool FlashlightTogglePressed { get; private set; }

    private int moveFingerId = -1;
    private int lookFingerId = -1;
    private Vector2 moveStart;

    private void Update()
    {
        LookDelta = Vector2.zero;
        InteractPressed = false;
        FlashlightTogglePressed = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Skip touches over UI so on-screen buttons do not leak into movement/look input.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                continue;
            }

            bool leftSide = touch.position.x <= Screen.width * 0.5f;

            if (touch.phase == TouchPhase.Began)
            {
                if (leftSide && moveFingerId == -1)
                {
                    moveFingerId = touch.fingerId;
                    moveStart = touch.position;
                }
                else if (!leftSide && lookFingerId == -1)
                {
                    lookFingerId = touch.fingerId;
                }
            }

            if (touch.fingerId == moveFingerId)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    moveFingerId = -1;
                    MoveInput = Vector2.zero;
                }
                else
                {
                    Vector2 delta = (touch.position - moveStart) / (Screen.width * 0.2f);
                    MoveInput = Vector2.ClampMagnitude(delta, 1f);

                    if (MoveInput.magnitude < deadZone)
                    {
                        MoveInput = Vector2.zero;
                    }
                }
            }

            if (touch.fingerId == lookFingerId)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    lookFingerId = -1;
                }
                else
                {
                    LookDelta = touch.deltaPosition * lookSensitivity * Time.deltaTime;
                }
            }
        }

        // Editor fallback for quick iteration.
#if UNITY_EDITOR
        if (Input.touchCount == 0)
        {
            MoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            LookDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
#endif
    }

    // UI Button Hooks
    public void SetSprintHeld(bool isHeld) => SprintHeld = isHeld;
    public void TriggerInteract() => InteractPressed = true;
    public void TriggerFlashlightToggle() => FlashlightTogglePressed = true;
}
