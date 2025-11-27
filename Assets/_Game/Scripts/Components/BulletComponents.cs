using Unity.Entities;
using Unity.Mathematics;

public struct Bullet : IComponentData {
    public float Speed;
    public float Damage;
    public float LifeTime;
    public float Radius;
}

public struct ToBeDestroyed : IComponentData { }

public struct ProjectileRegistry : IComponentData
{
    public Entity BulletPrefab;
}