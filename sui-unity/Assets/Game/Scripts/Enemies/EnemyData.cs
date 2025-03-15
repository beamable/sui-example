using System;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Enemies
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Moe/Data/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Serializable]
        public enum EnemyAttackType
        {
            None,
            Dash,
            Melee,
            Ranged
        }
        
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public int XpValue { get; private set; } = 25;
        
        [Header("Attack")]
        [field: SerializeField] public int AttackPower { get; private set; }
        [field: SerializeField] public float AttackRange { get; private set; }
        [field: SerializeField] public float AttackCooldown { get; private set; }
        [field: SerializeField] public EnemyAttackType AttackType { get; private set; }
        
    }
}