using UnityEngine;

public class FishController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float changeDirectionTimer = 2f;
    [SerializeField] private float swimSpeed = 2f;

    [Header("Bubble Settings")]
    [Tooltip("Drag your mouth Particle System here")]
    [SerializeField] private ParticleSystem bubbleParticles;
    [Tooltip("How fast must the fish be moving to emit bubbles?")]
    [SerializeField] private float movementThreshold = 0.5f;
    [Tooltip("How many bubbles spawn per second while swimming?")]
    [SerializeField] private int bubbleEmissionRate = 25;

    private Rigidbody rb;
    private float targetYRotation;
    private float timer;
    private ParticleSystem.EmissionModule emissionModule;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetYRotation = transform.eulerAngles.y;

        // Cache the particle emission system
        if (bubbleParticles != null)
        {
            emissionModule = bubbleParticles.emission;
        }
        else
        {
            Debug.LogWarning("Please assign the Particle System to the FishController script in the Inspector!", this);
        }
    }

    void Update()
    {
        // Handle Direction Timer
        timer += Time.deltaTime;
        if (timer > changeDirectionTimer)
        {
            float offset = Random.Range(-50f, 50f);
            targetYRotation = transform.eulerAngles.y + offset;
            timer = 0f;
        }

        // Handle Smooth Rotation
        float smoothRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation, Time.deltaTime * turnSpeed);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, smoothRotation, transform.eulerAngles.z);

        // Handle Forward Velocity using your swimSpeed variable
        Vector3 forwardVelocity = transform.forward * swimSpeed;
        rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);

        // Handle Bubble Emission based on Rigidbody physical speed
        HandleBubbleEmission();
    }

    private void HandleBubbleEmission()
    {
        if (bubbleParticles == null) return;

        // Get actual physical velocity speed from the Rigidbody
        float currentPhysicalSpeed = rb.linearVelocity.magnitude;

        // Toggle bubbles based on movement threshold
        if (currentPhysicalSpeed > movementThreshold)
        {
            emissionModule.rateOverTime = bubbleEmissionRate;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }
    }
}
