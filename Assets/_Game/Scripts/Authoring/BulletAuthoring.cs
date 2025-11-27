using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float Speed = 20f;
    public float Damage = 10f;
    public float LifeTime = 3f;
    public float HitRadius = 0.5f;

    class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Bullet
            {
                Speed = authoring.Speed,
                Damage = authoring.Damage,
                LifeTime = authoring.LifeTime,
                Radius = authoring.HitRadius
            });
        }
    }
}
