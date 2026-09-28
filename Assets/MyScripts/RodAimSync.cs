using UnityEngine;

public class RodAimSync : MonoBehaviour
{
    [Header("References")]
    public Transform rod; // "Fishing ROad"
    public MyPlayerCameraControllr cameraController;

    [Header("Tuning")]
    public float moveMultiplier = -0.01f; // how much the rod shifts per degree of camera pitch
    public float baseOffset = 0f;

    [Header("Axis (local space) to move the rod along")]
    public Vector3 moveAxis = Vector3.up;

    private Vector3 lastAppliedOffset = Vector3.zero;

    void LateUpdate()
    {
        if (rod == null || cameraController == null) return;

        float pitch = cameraController.VerticalRotation;
        float amount = pitch * moveMultiplier + baseOffset;

        Vector3 newOffset = moveAxis.normalized * amount;

        // Remove last frame's offset, then apply this frame's, so we don't stack on top of the animated pose
        rod.localPosition = rod.localPosition - lastAppliedOffset + newOffset;
        lastAppliedOffset = newOffset;
    }
}