using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public partial struct BulletSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // 1. MOVE BULLETS & HANDLE LIFETIME
        foreach (var (transform, bullet, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Bullet>>().WithEntityAccess())
        {
            // Move forward
            transform.ValueRW.Position += transform.ValueRO.Forward() * bullet.ValueRO.Speed * dt;

            // Reduce Life
            bullet.ValueRW.LifeTime -= dt;
            if (bullet.ValueRO.LifeTime <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }

        // 2. COLLISION DETECTION (Brute Force Distance Check)
        // Note: For 10,000+ units, we would use Spatial Hashing, but for < 2000 this is fine on Burst.

        // Query all Bullets and Enemies into NativeArrays for parallel access
        var bulletQuery = SystemAPI.QueryBuilder().WithAll<Bullet, LocalTransform>().Build();
        var enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyStats, LocalTransform>().Build();

        if (bulletQuery.IsEmpty || enemyQuery.IsEmpty) return;

        NativeArray<Entity> bulletEntities = bulletQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> bulletTransforms = bulletQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        NativeArray<Bullet> bulletDatas = bulletQuery.ToComponentDataArray<Bullet>(Allocator.Temp);

        NativeArray<Entity> enemyEntities = enemyQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        // Loop through every bullet
        for (int b = 0; b < bulletEntities.Length; b++)
        {
            float3 bPos = bulletTransforms[b].Position;
            float bRad = bulletDatas[b].Radius;
            bool hit = false;

            // Loop through every enemy
            for (int e = 0; e < enemyEntities.Length; e++)
            {
                float3 ePos = enemyTransforms[e].Position;

                // Simple Distance Check (Squared is faster)
                float distSq = math.distancesq(bPos, ePos);
                float combinedRadius = bRad + 0.5f; // Assume enemy radius is roughly 0.5f

                if (distSq < combinedRadius * combinedRadius)
                {
                    // HIT!
                    ecb.DestroyEntity(bulletEntities[b]); // Destroy Bullet
                    ecb.DestroyEntity(enemyEntities[e]);  // Destroy Enemy (Instant kill for now)
                    hit = true;
                    break; // Bullet can only hit one enemy
                }
            }
        }

        bulletEntities.Dispose();
        bulletTransforms.Dispose();
        bulletDatas.Dispose();
        enemyEntities.Dispose();
        enemyTransforms.Dispose();
    }
}