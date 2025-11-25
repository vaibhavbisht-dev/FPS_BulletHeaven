using Unity.Entities;
using UnityEngine;


public struct EnemyStats : IComponentData
{
    public float moveSpeed;
    public float health;
    public float damage;
}

public class EnemyAuthoring : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float health = 100f;
    public float damage = 10f;

    // The Baker converts the MonoBehaviour to ECS Data during the build
    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyStats
            {
                moveSpeed = authoring.moveSpeed,
                health = authoring.health,
                damage = authoring.damage
            });
        }
    }
}
