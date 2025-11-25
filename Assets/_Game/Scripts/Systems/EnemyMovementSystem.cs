using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        float3 targetPos = float3.zero; // Eventually, set this to Player Position

        // Iterate over all entities that have Transform and EnemyStats
        foreach (var (transform, stats) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyStats>>())
        {
            // 1. Calculate Direction
            float3 direction = math.normalize(targetPos - transform.ValueRO.Position);

            // 2. Move
            transform.ValueRW.Position += direction * stats.ValueRO.moveSpeed * dt;

            // 3. Rotate to face target
            if (!direction.Equals(float3.zero))
            {
                transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
            }
        }
    }
}