using UnityEngine;
using UnityEngine.EventSystems;

public class MyPlayerCameraControllr : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float sensitivity = 0.5f;
    public float minVerticalAngle = -70f;
    public float maxVerticalAngle = 70f;

    [Header("Touch Input (editor / fallback)")]
    public FixedTouchField touchField;
    public FixedJoystick joystickReference; // Drag your joystick here to prevent input overlap

    [Header("Anti-Spike Settings")]
    [Tooltip("Max raw pixel delta accepted per frame. Stops finger-swap / re-acquire jumps from flinging the camera.")]
    public float maxDeltaPerFrame = 40f;

    [Header("Player Reference")]
    public Rigidbody playerRb;

    private float horizontalRotation;
    private float verticalRotation;

    // Manual touch tracking
    private int lookFingerId = -1;

    public float VerticalRotation => verticalRotation;

    void Start()
    {
        verticalRotation = 0f;
        horizontalRotation = playerRb.rotation.eulerAngles.y;

        transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
        playerRb.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    void Update()
    {
        if (Input.touchSupported && Input.touchCount > 0)
        {
            HandleMultiTouchLook();
        }
        else
        {
            HandleTouchFieldOrMouseLook();
        }
    }

    // --- Real multitouch path: isolates the joystick's finger from the look finger ---
    private void HandleMultiTouchLook()
    {
        // Keep using the same finger for look until it lifts, so we don't
        // swap references mid-drag (that swap is what causes big jumps).
        if (lookFingerId != -1)
        {
            bool stillActive = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != lookFingerId) continue;

                stillActive = true;

                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    ApplyLookDelta(t.deltaPosition);
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    lookFingerId = -1;
                }

                break;
            }

            if (!stillActive) lookFingerId = -1;
            return;
        }

        // Look for a fresh touch to claim for camera look.
        // Skip any touch that began over a UI element (the joystick, buttons, etc.)
        // — this is the key fix: the joystick's own finger can never hijack the camera.
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

            lookFingerId = t.fingerId;
            break;
        }
    }

    // --- Fallback path (editor testing with FixedTouchField / mouse) ---
    private void HandleTouchFieldOrMouseLook()
    {
        if (joystickReference != null && (joystickReference.Horizontal != 0f || joystickReference.Vertical != 0f))
        {
            // Joystick is actively being dragged - don't let the touch field fight it.
            return;
        }

        if (touchField != null)
        {
            ApplyLookDelta(touchField.TouchDist);
        }
    }

    private void ApplyLookDelta(Vector2 rawDelta)
    {
        // Hard clamp so a single bad-frame spike can't fling the camera around.
        rawDelta = Vector2.ClampMagnitude(rawDelta, maxDeltaPerFrame);

        horizontalRotation += rawDelta.x * sensitivity;
        verticalRotation -= rawDelta.y * sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        playerRb.MoveRotation(targetRotation);
    }
}