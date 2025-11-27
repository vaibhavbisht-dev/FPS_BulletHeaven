using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Settings")]
    public Transform firePoint; // Assign a child object where bullets spawn
    public float fireRate = 0.2f;

    [Header("Input")]
    public PlayerInputManager inputManager; // Reference your existing input script

    private EntityManager entityManager;
    private float nextFireTime;
    private bool registryFound = false;
    private Entity bulletPrefab;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        // 1. Try to find the Bullet Prefab from ECS (Only needs to happen once)
        if (!registryFound)
        {
            if (TryGetBulletPrefab())
            {
                registryFound = true;
                Debug.Log("Gun Loaded: Connected to ECS Projectile System");
            }
            return;
        }

        // 2. Handle Shooting Input
        // Note: You might need to add a "Fire" action to your PlayerInputManager
        // For now, we'll check Mouse Button directly or use Jump as a test trigger
        bool isFiring = inputManager.IsFiring;

        if (isFiring && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // Spawn the bullet entity
        Entity bullet = entityManager.Instantiate(bulletPrefab);

        // Set Position and Rotation to match the Gun
        entityManager.SetComponentData(bullet, new LocalTransform
        {
            Position = (float3)firePoint.position,
            Rotation = (quaternion)firePoint.rotation,
            Scale = 1f
        });
    }

    // Helper to fetch the singleton
    private bool TryGetBulletPrefab()
    {
        try
        {
            // Query for the Registry we created
            EntityQuery query = entityManager.CreateEntityQuery(typeof(ProjectileRegistry));
            if (query.IsEmpty) return false;

            var registry = query.GetSingleton<ProjectileRegistry>();
            bulletPrefab = registry.BulletPrefab;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
