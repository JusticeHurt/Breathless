using UnityEngine;
using UnityEngine.AI;

public class DeerAI : MonoBehaviour
{
    public enum DeerState { Idle, Wander, Fleeing }
    
    [Header("Status")]
    public DeerState currentState = DeerState.Idle;

    [Header("Movement Settings")]
    public float walkSpeed = 1f;
    public float fleeSpeed = 7f;
    public float wanderRadius = 15f; 
    public float detectionRange = 25f;
    public float stillDetectionRange = 10f; 
    
    [Header("Timing")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 6f;
    private float timer;

    private NavMeshAgent agent;
    private Transform player;
    private Hunter hunterScript; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
            hunterScript = playerObj.GetComponent<Hunter>();
        }

        timer = Random.Range(minIdleTime, maxIdleTime);
        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (player == null || hunterScript == null) return;

        HandleDetection();

        switch (currentState)
        {
            case DeerState.Idle: HandleIdle(); break;
            case DeerState.Wander: HandleWander(); break;
            case DeerState.Fleeing: HandleFleeing(); break;
        }
    }

    void HandleDetection()
    {
        if (currentState == DeerState.Fleeing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float noise = hunterScript.currentNoise;

        // HUGE noise the deer hears it 
        // regardless of walls/vision as long as it's within range.
        if (noise > 30f && distanceToPlayer < 35f)
        {
            Debug.Log(gameObject.name + " heard a loud shot!");
            OnHeardNoise(player.position);
            return;
        }

        // If noise is > 5 (walking), use detectionRange. If still/holding breath, use stillRange.
        float currentDetectionRange = (noise > 5f) ? detectionRange : stillDetectionRange;

        if (distanceToPlayer < currentDetectionRange)
        {
            Vector3 rayOrigin = transform.position + (Vector3.up * 1.6f);
            Vector3 dirToPlayer = (player.position - rayOrigin).normalized;

            if (Physics.Raycast(rayOrigin, dirToPlayer, out RaycastHit hit, currentDetectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    OnHeardNoise(player.position);
                }
            }
        }
    }

    public void OnHeardNoise(Vector3 noiseSource)
    {
        if (currentState == DeerState.Fleeing) return;

        currentState = DeerState.Fleeing;
        agent.speed = fleeSpeed;

        Vector3 fleeDir = (transform.position - noiseSource).normalized;
        Vector3 rawTargetPos = transform.position + fleeDir * 30f; 

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTargetPos, out hit, 15f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(transform.position + fleeDir * 10f);
        }
    }

    void HandleFleeing()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            // Keep fleeing if still too close
            if (distToPlayer < 20f) 
            {
                OnHeardNoise(player.position); 
            }
            else 
            {
                timer = maxIdleTime;
                currentState = DeerState.Idle;
                agent.speed = walkSpeed;
            }
        }
    }

    void HandleIdle()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Vector3 newTarget = GetRandomNavPos();
            if (newTarget != Vector3.zero)
            {
                agent.SetDestination(newTarget);
                currentState = DeerState.Wander;
            }
            else { timer = 1f; }
        }
    }

    void HandleWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer = Random.Range(minIdleTime, maxIdleTime);
            currentState = DeerState.Idle;
        }
    }

    Vector3 GetRandomNavPos()
    {
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, wanderRadius, 1)) return hit.position;
        return Vector3.zero;
    }

}