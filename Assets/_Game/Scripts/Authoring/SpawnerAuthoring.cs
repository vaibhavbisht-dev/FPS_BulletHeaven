using Unity.Entities;
using UnityEngine;

// Data for the Spawner
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
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRateSeconds = 1f;
    [SerializeField] private float minRadius = 15f;
    [SerializeField] private float maxRadius = 25f;
    [SerializeField] private float difficultyScaling = 0.5f;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemySpawner
            {
                enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                spawnRate = authoring.spawnRateSeconds,
                timer = 0f,
                minRadius = authoring.minRadius,
                maxRadius = authoring.maxRadius,
                difficultyMultiplier = authoring.difficultyScaling
            });
        }
    }
}