using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class FishingRod : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform tip;
    public Transform cameraTransform;
    public Transform playerTransform;
    public LayerMask whatIsGrappleable;

    [Header("Fishing Settings")]
    public float castDistance = 15f;
    public float lineShootSpeed = 20f;
    public float reelSpeed = 8f;
    public float catchRadius = 1.5f;
    public LayerMask fishLayer;

    [Header("Rope Visuals")]
    [Range(10, 50)] public int ropeSegments = 25;
    public float ropeDroop = 3f;

    [Header("Water Settings")]
    public float waterSurfaceY = 11.03f;
    public bool keepFishInWater = true;

    [Header("Events")]
    public UnityEvent<GameObject> onFishCaught;

    [Header("Creature Caught Text")]
    public GameObject creatureCaughtPanel;

    [Header("Fish Sell/Eat Panel")]
    public GameObject fishCaughtPanel; // panel with Sell + Eat buttons
    public Button sellButton;
    public Button eatButton;

    [Header("Coin Settings")]
    public int coins = 0;
    public TMP_Text coinText; // drag your coin count UI text here
    public int coinsPerSell = 500;

    [Header("Hunger Settings")]
    public HungerManager hungerManager;
    public float hungerPerEat = 5f;

    private Vector3 targetGrapplePoint;
    private Vector3 currentLineTipPosition;
    private Transform caughtFish;
    private GameObject pendingFish; // fish waiting for sell/eat decision

    private bool isShooting = false;
    private bool isReeling = false;
    private bool hasCaughtFish = false;
    private bool isLineIdle = false;

    void Awake()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
    }

    void Start()
    {
        UpdateCoinUI();

        if (fishCaughtPanel != null)
            fishCaughtPanel.SetActive(false);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellButtonPressed);

        if (eatButton != null)
            eatButton.onClick.AddListener(OnEatButtonPressed);
    }

    void Update()
    {
        if (isShooting)
            MoveLineForward();

        if (hasCaughtFish && caughtFish != null)
            targetGrapplePoint = caughtFish.position;

        if (hasCaughtFish && isReeling && caughtFish != null)
            ReelFishIn();
    }

    void LateUpdate()
    {
        DrawCurvedRope();
    }

    public void OnCastButtonPressed()
    {
        if (isShooting || hasCaughtFish || isLineIdle)
        {
            ResetFishingLine();
            return;
        }

        StartShootingLine();
    }

    public void OnReelButtonDown()
    {
        if (hasCaughtFish)
        {
            isReeling = true;
            creatureCaughtPanel.gameObject.SetActive(false);
        }
        else if (isLineIdle)
        {
            ResetFishingLine();
        }
    }

    public void OnReelButtonUp()
    {
        isReeling = false;
    }

    void StartShootingLine()
    {
        LaunchRope();

        float currentThrowDistance = Vector3.Distance(cameraTransform.position, targetGrapplePoint);

        Ray ray = new Ray(cameraTransform.position, (targetGrapplePoint - cameraTransform.position).normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, currentThrowDistance, whatIsGrappleable, QueryTriggerInteraction.Collide))
        {
            targetGrapplePoint = hit.point;
        }

        caughtFish = null;
        isShooting = true;
        hasCaughtFish = false;
        isReeling = false;
        isLineIdle = false;
        currentLineTipPosition = tip.position;

        lr.positionCount = ropeSegments;
    }

    void MoveLineForward()
    {
        currentLineTipPosition = Vector3.MoveTowards(currentLineTipPosition, targetGrapplePoint, lineShootSpeed * Time.deltaTime);

        if (caughtFish == null)
        {
            Collider[] nearby = Physics.OverlapSphere(currentLineTipPosition, catchRadius, fishLayer, QueryTriggerInteraction.Collide);

            if (nearby.Length > 0)
            {
                caughtFish = nearby[0].transform;
                targetGrapplePoint = caughtFish.position;
                isShooting = false;
                hasCaughtFish = true;
                creatureCaughtPanel.gameObject.SetActive(true);
                return;
            }
        }

        if (Vector3.Distance(currentLineTipPosition, targetGrapplePoint) < 0.1f)
        {
            isShooting = false;

            if (caughtFish != null)
                hasCaughtFish = true;
            else
                isLineIdle = true;
        }
    }

    void ReelFishIn()
    {
        Vector3 target = new Vector3(tip.position.x, caughtFish.position.y, tip.position.z);
        caughtFish.position = Vector3.MoveTowards(caughtFish.position, target, reelSpeed * Time.deltaTime);

        if (keepFishInWater && caughtFish.position.y > waterSurfaceY)
        {
            Vector3 pos = caughtFish.position;
            pos.y = waterSurfaceY;
            caughtFish.position = pos;
        }

        float horizontalDist = Vector2.Distance(
            new Vector2(caughtFish.position.x, caughtFish.position.z),
            new Vector2(tip.position.x, tip.position.z)
        );

        if (horizontalDist < 0.5f)
        {
            CatchSuccess();
        }
    }

    void CatchSuccess()
    {
        GameObject fishObj = caughtFish.gameObject;
        onFishCaught?.Invoke(fishObj);

        pendingFish = fishObj;
        Destroy(fishObj);

        if (fishCaughtPanel != null)
            fishCaughtPanel.SetActive(true);

        ResetFishingLine();
    }

    public void OnSellButtonPressed()
    {
        coins += coinsPerSell;
        UpdateCoinUI();

        CloseFishPanel();
    }

    public void OnEatButtonPressed()
    {
        if (hungerManager != null)
        {
            hungerManager.AddHunger(hungerPerEat);
        }

        CloseFishPanel();
    }

    void CloseFishPanel()
    {
        pendingFish = null;

        if (fishCaughtPanel != null)
            fishCaughtPanel.SetActive(false);
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString();
    }

    void ResetFishingLine()
    {
        isShooting = false;
        hasCaughtFish = false;
        isReeling = false;
        isLineIdle = false;
        caughtFish = null;
        lr.positionCount = 0;
    }

    void DrawCurvedRope()
    {
        if (!isShooting && !hasCaughtFish && !isLineIdle)
        {
            lr.positionCount = 0;
            return;
        }

        lr.positionCount = ropeSegments;
        Vector3 start = tip.position;
        Vector3 end = isShooting ? currentLineTipPosition : targetGrapplePoint;

        float distance = Vector3.Distance(start, end);

        if (distance < 0.1f)
        {
            for (int i = 0; i < ropeSegments; i++)
            {
                float t = i / (float)(ropeSegments - 1);
                lr.SetPosition(i, Vector3.Lerp(start, end, t));
            }
            return;
        }

        float activeDroop = isReeling ? 50f : Mathf.Max(0.1f, ropeDroop);
        float parameterA = activeDroop * (distance + 1f);

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector3 linearPoint = Vector3.Lerp(start, end, t);

            float x = (t - 0.5f) * distance;
            float catenaryY = parameterA * (float)System.Math.Cosh(x / parameterA);
            float baselineY = parameterA * (float)System.Math.Cosh((0.5f * distance) / parameterA);
            float sagOffset = catenaryY - baselineY;

            Vector3 ropePoint = linearPoint + (Vector3.down * -sagOffset);
            lr.SetPosition(i, ropePoint);
        }
    }

    void LaunchRope()
    {
        Plane waterPlane = new Plane(Vector3.up, new Vector3(0f, waterSurfaceY, 0f));
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        float rayHitDistance;

        if (waterPlane.Raycast(ray, out rayHitDistance))
        {
            Vector3 waterAimPoint = ray.GetPoint(rayHitDistance);

            Vector3 horizontalDir = waterAimPoint - cameraTransform.position;
            horizontalDir.y = 0f;
            horizontalDir.Normalize();

            targetGrapplePoint = cameraTransform.position + horizontalDir * castDistance;
            targetGrapplePoint.y = waterSurfaceY;
        }
        else
        {
            targetGrapplePoint = cameraTransform.position + (cameraTransform.forward * castDistance);
            targetGrapplePoint.y = waterSurfaceY;
        }
    }
}