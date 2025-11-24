using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClassData", menuName = "Game/CharacterClassData")]
public class CharacterClassData : ScriptableObject
{
    public string className; // "Wizard", "Assassin"
    public float baseMoveSpeed;
    public float baseHealth;

    [Header("Attack Config")]
    public GameObject projectilePrefab; // The fireball or dagger
    public float fireRate;
    public float damage;
}
