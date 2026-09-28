using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshWanderer : MonoBehaviour
{
    public Animator animator;
    public float wanderRadius = 15f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private float waitTimer;
    private bool waiting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        TryPlaceOnNavMesh();
        SetNewDestination();
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            TryPlaceOnNavMesh();
            return;
        }

        bool isWalking = agent.velocity.sqrMagnitude > 0.01f;
        //if (animator != null)
        //    animator.SetBool("isWalking", isWalking);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!waiting)
            {
                waiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                SetNewDestination();
            }
        }
    }

    void TryPlaceOnNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: no NavMesh found within 10 units of spawn position {transform.position}. Move it closer to the island.");
        }
    }

    void SetNewDestination()
    {
        if (!agent.isOnNavMesh) return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}