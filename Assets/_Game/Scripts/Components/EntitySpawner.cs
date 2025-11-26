// 26-11-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using Unity.Entities;
class EntitySpawner
{
    
    public struct EnemySpawner : IComponentData
    {
        public Entity enemyPrefab;
        public float spawnRate;
        public float timer;
        public float minRadius;
        public float maxRadius;
        public float difficultyMultiplier;
    }
}
