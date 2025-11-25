using Unity.Entities;
using UnityEngine;


public struct EnemySpawner : IComponentData
{
    public Entity enemyPrefab; // The specific enemy to spawn
    public float spawnRate;    // Seconds between spawns
    public float timer;        // Internal timer

    // Spawning Area (The "Donut")
    public float minRadius;
    public float maxRadius;

    // Difficulty Scaling
    public float difficultyMultiplier; // How much faster it gets per minute
}

public class SpawnerAuthoring : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public GameObject enemyPrefab; // Drag your Prefab here

    [Header("Spawn Settings")]
    public float spawnRateSeconds = 1.0f;
    public float minRadius = 15f; // Just outside camera view
    public float maxRadius = 25f;

    [Header("Difficulty")]
    [Tooltip("Reduces spawn timer by this % every 60 seconds. 0.1 = 10% faster per minute.")]
    public float difficultyScaling = 0.1f;

    public class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Register the prefab as a dependency so it bakes correctly
            AddComponent(entity, new EnemySpawner
            {
                enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                spawnRate = authoring.spawnRateSeconds,
                timer = 0,
                minRadius = authoring.minRadius,
                maxRadius = authoring.maxRadius,
                difficultyMultiplier = authoring.difficultyScaling
            });
        }
    }
}
