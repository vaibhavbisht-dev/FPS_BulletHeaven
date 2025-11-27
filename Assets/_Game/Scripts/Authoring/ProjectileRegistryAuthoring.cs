using Unity.Entities;
using UnityEngine;

public class ProjectileRegistryAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefab;

    class Baker : Baker<ProjectileRegistryAuthoring>
    {
        public override void Bake(ProjectileRegistryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ProjectileRegistry
            {
                BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
