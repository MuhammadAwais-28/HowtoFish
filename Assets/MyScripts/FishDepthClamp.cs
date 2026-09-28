using UnityEngine;

public class FishDepthClamp : MonoBehaviour
{
    [Header("Water Depth Settings")]
    public float waterSurfaceY = 11.03f;
    public float minDepthBelowSurface = 0.5f;
    public float maxDepthBelowSurface = 3f;

    private Rigidbody rb;
    private bool clampEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    public void SetClampEnabled(bool enabled)
    {
        clampEnabled = enabled;
    }

    void LateUpdate()
    {
        if (!clampEnabled) return;

        Vector3 pos = transform.position;

        float minY = waterSurfaceY - maxDepthBelowSurface;
        float maxY = waterSurfaceY - minDepthBelowSurface;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}