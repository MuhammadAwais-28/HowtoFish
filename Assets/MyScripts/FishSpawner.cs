using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Setup")]
    public GameObject[] fishPrefabs;
    public int fishCount = 60;
    public Transform fishParent;

    [Header("Spawn Area (water region around island)")]
    public Vector3 spawnAreaCenter = new Vector3(504f, 0f, 500f);
    public Vector2 spawnAreaSize = new Vector2(10f, 10f); // width (X) and depth (Z)

    [Header("Water Placement")]
    public float waterSurfaceY = 0f;
    public float minDepthBelowSurface = 0.5f;
    public float maxDepthBelowSurface = 3f;

    [Header("Land Avoidance")]
    public float minDistanceFromLand = 3f; // buffer so fish don't spawn right at the shoreline

    [Header("Respawn Check")]
    public float checkInterval = 1f; // how often to check for missing fish

    private List<GameObject> activeFish = new List<GameObject>();
    private float checkTimer = 0f;

    void Start()
    {
        SpawnFish(fishCount);
    }

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            TopUpFish();
        }
    }

    void TopUpFish()
    {
        // Remove destroyed/null entries (caught fish)
        activeFish.RemoveAll(f => f == null);

        int missing = fishCount - activeFish.Count;
        if (missing > 0)
        {
            SpawnFish(missing);
        }
    }

    void SpawnFish(int amount)
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = amount * 30;

        while (spawned < amount && attempts < maxAttempts)
        {
            attempts++;

            float x = spawnAreaCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float z = spawnAreaCenter.z + Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            Vector3 checkPoint = new Vector3(x, waterSurfaceY, z);

            if (IsNearLand(checkPoint))
                continue;

            float depth = Random.Range(minDepthBelowSurface, maxDepthBelowSurface);
            Vector3 spawnPos = new Vector3(x, waterSurfaceY - depth, z);

            GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
            GameObject fish = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            if (fishParent != null)
                fish.transform.SetParent(fishParent);

            activeFish.Add(fish);
            spawned++;
        }

        if (spawned < amount)
        {
            Debug.LogWarning($"FishSpawner: only placed {spawned}/{amount} fish after {attempts} attempts. " +
                              $"Try increasing spawnAreaSize, or reducing minDistanceFromLand.");
        }
    }

    bool IsNearLand(Vector3 point)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(point, out hit, minDistanceFromLand, NavMesh.AllAreas);
    }
}