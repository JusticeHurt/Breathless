using UnityEngine;
using UnityEngine.AI;

public class MonsterStalker : MonoBehaviour 
{
    public enum MonsterState { Wander, Stalking, Hunting, Searching }
    
    [Header("Status")]
    public MonsterState currentState = MonsterState.Wander;

    [Header("Movement")]
    public float searchSpeed = 3f;
    public float stalkSpeed = 2.5f; 
    public float chaseSpeed = 6f; 
    public float maxChaseSpeed = 12f; 
    public float chaseAcceleration = 1.5f; 
    
    [Header("Vision Settings")]
    public float visionRange = 10f; 
    public float fieldOfView = 90f; 
    public LayerMask obstacleMask;

    [Header("Combat Settings")]
    public float attackDamage = 40f;     
    public float attackRange = 2.5f;     // Hhw close it must be to hit
    public float attackCooldown = 1.5f;  // time between swings
    private float lastAttackTime;

    [Header("Search Settings")]
    public float searchDuration = 10f; 
    private float searchTimer;

    [Header("Senses (Base Values)")]
    public float baseHearingThreshold = 12f;    
    public float baseHuntThreshold = 20f;       
    public float instantDetectNoise = 40f;      

    [Header("Distance Settings")]
    public float wanderRadius = 15f;

    private NavMeshAgent agent;
    private Transform player;
    private Hunter hunterScript;
    private PlayerHealth playerHealth; // reference to the health script
    private Vector3 lastHeardPosition;
    private float currentChaseSpeed;

    [Header("Distance Settings")]
    public float tetherDistance = 40f; // distance before the monster rubber bands to the player

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
            hunterScript = playerObj.GetComponent<Hunter>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            lastHeardPosition = transform.position; 
        }
    }

    void Update()
    {
        if (player == null || hunterScript == null) return;

        HandleSenses();

        switch (currentState)
        {
            case MonsterState.Wander: HandleWandering(); break;
            case MonsterState.Stalking: HandleStalking(); break;
            case MonsterState.Hunting: HandleHunting(); break;
            case MonsterState.Searching: HandleSearching(); break;
        }

        // check if close enough to hurt the player
        HandleCombat();
    }

    void HandleCombat()
    {
        // Don't attack if player is already dead or script is missing
        if (playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        playerHealth.TakeDamage(attackDamage);
        
        Debug.Log("Monster Attacked! Player Health: " + playerHealth.currentHealth);
        
        // i still need to  Add a 'Scream' or 'Hit' sound here to get Dynamic Audio (3 pts)
    }

    void HandleSenses()
    {
        float noise = hunterScript.currentNoise;
        bool canSeePlayer = CheckVisualDetection();

        float sensitivity = Mathf.Pow(0.85f, Hunter.deerKilled);
        float huntThreshold = baseHuntThreshold * sensitivity;
        float hearingThreshold = baseHearingThreshold * sensitivity;

        if (canSeePlayer || noise > huntThreshold || noise > instantDetectNoise)
        {
            lastHeardPosition = player.position;
            
            if (currentState != MonsterState.Hunting)
            {
                currentState = MonsterState.Hunting;
                currentChaseSpeed = chaseSpeed;
            }
        }
        else if (noise > hearingThreshold && currentState != MonsterState.Hunting)
        {
            lastHeardPosition = player.position;
            currentState = MonsterState.Stalking;
        }
        else
        {
            if (currentState == MonsterState.Hunting || currentState == MonsterState.Stalking)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                {
                    searchTimer = searchDuration;
                    currentState = MonsterState.Searching;
                }
            }
        }
    }

    bool CheckVisualDetection()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < visionRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) < fieldOfView / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, dir, dist, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void HandleStalking()
    {
        agent.isStopped = false;
        agent.speed = stalkSpeed;
        agent.SetDestination(lastHeardPosition);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            searchTimer = searchDuration / 2f;
            currentState = MonsterState.Searching;
        }
    }

    void HandleHunting()
    {
        agent.isStopped = false;
        agent.SetDestination(lastHeardPosition);

        currentChaseSpeed += chaseAcceleration * Time.deltaTime;
        float speedMod = 1.0f + (Hunter.deerKilled * 0.2f);
        agent.speed = Mathf.Min(currentChaseSpeed * speedMod, maxChaseSpeed * speedMod);
    }

    void HandleSearching()
    {
        agent.isStopped = false;
        agent.speed = searchSpeed;
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0)
        {
            currentState = MonsterState.Wander;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            Vector3 point = lastHeardPosition + (Random.insideUnitSphere * 8f);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 8f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    void HandleWandering()
    {
        agent.isStopped = false;
        agent.speed = searchSpeed * 0.7f;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            Vector3 centerPoint;
            
            // distance check the monster is from the player
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distToPlayer > tetherDistance)
            {
                //monster is too far away! cheat and wander toward the player zone.
                centerPoint = player.position;
            }
            else
            {
                // The monster is in the neighborhood! Wander naturally around itself
                centerPoint = transform.position;
            }

            // Pick a random point around the random chosen center
            Vector3 point = centerPoint + (Random.insideUnitSphere * wanderRadius);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }




}