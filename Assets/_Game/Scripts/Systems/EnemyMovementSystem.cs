using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<PlayerTargetData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        float3 targetPos = SystemAPI.GetSingleton<PlayerTargetData>().Position; // Eventually, set this to Player Position

        // Iterate over all entities that have Transform and EnemyStats
        foreach (var (transform, stats) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyStats>>())
        {
            // Calculate Direction to Player
            float3 dirVector = targetPos - transform.ValueRO.Position;

            // Flatten Y so they don't look up/down
            dirVector.y = 0;

            // Normalize
            float3 direction = math.normalize(dirVector);

            // Move
            transform.ValueRW.Position += direction * stats.ValueRO.moveSpeed * dt;

            // Rotate
            if (!direction.Equals(float3.zero))
            {
                transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
            }
        }
    }
}