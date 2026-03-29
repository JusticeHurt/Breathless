using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthInMinutes = 5f; 
    [Range(0, 1)] public float timeOfDay = 0.25f; 

    [Header("Spawning & Prefabs")]
    public GameObject dayDeerPrefab;
    public GameObject monsterPrefab;
    public Transform player;
    public int totalDeerGoal = 10; 

    [Header("Lighting")]
    public float maxIntensity = 1.2f;
    public float minIntensity = 0f;
    private Light sunLight;

    private bool isNight = false;
    private List<GameObject> activeSpawns = new List<GameObject>();

    void Start()
    {
        sunLight = GetComponent<Light>();
        
        // Find the player in the scene so spawning follows you
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
        }

        TransitionToDay();
    }

    void Update()
    {
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay >= 1) timeOfDay = 0;

        float sunRotation = (timeOfDay * 360f) - 90f;
        transform.localRotation = Quaternion.Euler(sunRotation, 170f, 0f);

        UpdateLightIntensity();
        CheckSpawnState();
    }

    void UpdateLightIntensity()
    {
        sunLight.intensity = (timeOfDay > 0.23f && timeOfDay < 0.73f) ? maxIntensity : minIntensity;
    }

    void CheckSpawnState()
    {
        bool currentlyNight = (timeOfDay <= 0.23f || timeOfDay >= 0.78f);

        if (currentlyNight && !isNight)
        {
            TransitionToNight();
        }
        else if (!currentlyNight && isNight)
        {
            TransitionToDay();
        }
    }

    void TransitionToNight()
    {
        isNight = true;
        ClearActiveSpawns();
        
        int remaining = totalDeerGoal - Hunter.deerKilled;
        if (remaining <= 0) return;

        for (int i = 0; i < remaining; i++) Spawn(monsterPrefab);
    }

    void TransitionToDay()
    {
        isNight = false;
        ClearActiveSpawns();

        int remaining = totalDeerGoal - Hunter.deerKilled;
        if (remaining <= 0) return;

        for (int i = 0; i < remaining; i++) Spawn(dayDeerPrefab);
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null || player == null) return;

        // Spawn around the player's CURRENT position
        Vector3 randomPos = player.position + (Random.insideUnitSphere * 40f);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, 50f, NavMesh.AllAreas))
        {
            activeSpawns.Add(Instantiate(prefab, hit.position, Quaternion.identity));
        }
    }

    void ClearActiveSpawns()
    {
        foreach (GameObject obj in activeSpawns) 
        {
            if (obj != null) Destroy(obj);
        }
        activeSpawns.Clear();
    }
}