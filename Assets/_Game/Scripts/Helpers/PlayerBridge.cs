using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerBridge : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity targetEntity;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        targetEntity = entityManager.CreateEntity(typeof(PlayerTargetData));
    }

    void Update()
    {
        if (entityManager.Exists(targetEntity))
        {
            entityManager.SetComponentData(targetEntity, new PlayerTargetData
            {
                Position = (float3)transform.position
            });
        }
    }

    void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld != null && entityManager.Exists(targetEntity))
        {
            entityManager.DestroyEntity(targetEntity);
        }
    }
}
