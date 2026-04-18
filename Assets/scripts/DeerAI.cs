using UnityEngine;
using UnityEngine.AI;

public class DeerAI : MonoBehaviour
{
    public enum DeerState { Idle, Wander, Fleeing }
    
    [Header("Status")]
    public DeerState currentState = DeerState.Idle;
    private DeerState lastState; // Tracks changes to prevent animation stuttering

    [Header("Movement Settings")]
    public float walkSpeed = 1f;
    public float fleeSpeed = 7f;
    public float wanderRadius = 15f; 
    public float detectionRange = 25f;
    public float stillDetectionRange = 10f; 
    
    [Header("Timing")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 15f;
    private float timer;

    private NavMeshAgent agent;
    private Transform player;
    private Hunter hunterScript; 
    private Animator animator; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
            hunterScript = playerObj.GetComponent<Hunter>();
        }

        // Initialize state
        lastState = currentState;
        timer = Random.Range(minIdleTime, maxIdleTime);
        agent.speed = walkSpeed;

        // Ensure agent doesn't get stuck on tiny bumps
        agent.stoppingDistance = 0.5f; 
    }

    void Update()
    {
        if (player == null || hunterScript == null) return;

        HandleDetection();
        UpdateAnimator();

        switch (currentState)
        {
            case DeerState.Idle: HandleIdle(); break;
            case DeerState.Wander: HandleWander(); break;
            case DeerState.Fleeing: HandleFleeing(); break;
        }
    }

    void UpdateAnimator()
    {
        if (animator != null && currentState != lastState)
        {
            // Only triggers when the state actually flips (Idle -> Wander, etc.)
            animator.SetInteger("State", (int)currentState);
            lastState = currentState;
        }
    }

    void HandleDetection()
    {
        if (currentState == DeerState.Fleeing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float noise = hunterScript.currentNoise;

        // Heard a shot (Loud noise)
        if (noise > 30f && distanceToPlayer < 35f)
        {
            OnHeardNoise(player.position);
            return;
        }

        // Determine range based on player movement
        float currentDetectionRange = (noise > 5f) ? detectionRange : stillDetectionRange;

        if (distanceToPlayer < currentDetectionRange)
        {
            // Raycast to check line of sight (deer head height approx 1.6f)
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
        currentState = DeerState.Fleeing;
        agent.speed = fleeSpeed;

        Vector3 fleeDir = (transform.position - noiseSource).normalized;
        Vector3 rawTargetPos = transform.position + fleeDir * 30f; 

        NavMeshHit hit;
        // Sample position to find a valid spot on the NavMesh to run to
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

            if (distToPlayer < 20f) 
            {
                OnHeardNoise(player.position); 
            }
            else 
            {
                timer = Random.Range(minIdleTime, maxIdleTime);
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
            else 
            { 
                timer = 1f; // Retry in 1 second if no valid spot found
            }
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
        // The '1' in the last parameter refers to the first NavMesh area (Walkable)
        if (NavMesh.SamplePosition(randomDir, out hit, wanderRadius, 1)) 
        {
            return hit.position;
        }
        return Vector3.zero;
    }
}