using UnityEngine;
using UnityEngine.AI;

public class TreeObstacleGenerator : MonoBehaviour
{
    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;

        foreach (TreeInstance tree in data.treeInstances)
        {
            // position of the tree
            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            //temporary GameObject to hold the obstacle
            GameObject obstacleFolder = new GameObject("TreeObstacle");
            obstacleFolder.transform.position = worldPos;
            obstacleFolder.transform.parent = this.transform;

            // Add the NavMesh Obstacle component
            NavMeshObstacle nmo = obstacleFolder.AddComponent<NavMeshObstacle>();
            nmo.shape = NavMeshObstacleShape.Capsule;
            
            nmo.radius = 3f;
            nmo.height = 4f;
            nmo.carving = true; 
        }
    }
}