using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemySpawnSystem : ISystem
{
    // Initialize Random Seed
    private Unity.Mathematics.Random rng;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawner>();
        rng = new Unity.Mathematics.Random(1234);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Get Player Position (Assume player is at 0,0,0 for now, or fetch Player Tag)
        // Ideally, you would have a "PlayerTag" component to fetch the exact position.
        float3 playerPos = float3.zero;

        // Example: Finding the player entity if you have a singleton tag
        // if (SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player))
        //     playerPos = SystemAPI.GetComponent<LocalTransform>(player).Position;

        float dt = SystemAPI.Time.DeltaTime;
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        // 2. Iterate over every Spawner in the scene
        foreach (var (spawner, localTransform) in SystemAPI.Query<RefRW<EnemySpawner>, RefRO<LocalTransform>>())
        {
            // Update Timer
            spawner.ValueRW.timer -= dt;

            // --- DIFFICULTY LOGIC ---
            // Calculate effective spawn rate based on time
            // Formula: Rate / (1 + (Scaling * MinutesPassed))
            float minutes = (float)elapsedTime / 60.0f;
            float currentRate = spawner.ValueRO.spawnRate / (1.0f + (spawner.ValueRO.difficultyMultiplier * minutes));

            // Cap it so it doesn't go to 0 (infinite spawn)
            currentRate = math.max(currentRate, 0.05f);

            if (spawner.ValueRO.timer <= 0)
            {
                
                // Reset Timer
                spawner.ValueRW.timer = currentRate;

                // --- SPAWN LOGIC ---
                Entity newEnemy = state.EntityManager.Instantiate(spawner.ValueRO.enemyPrefab);

                // Calculate Random Position in Donut
                float angle = rng.NextFloat(0, math.PI * 2);
                float distance = rng.NextFloat(spawner.ValueRO.minRadius, spawner.ValueRO.maxRadius);

                float x = math.cos(angle) * distance;
                float z = math.sin(angle) * distance;

                float3 spawnPos = playerPos + new float3(x, 1, z);

                // Set Position
                state.EntityManager.SetComponentData(newEnemy, LocalTransform.FromPosition(spawnPos));
            }
        }
    }
}