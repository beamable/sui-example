using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Data
{
    [CreateAssetMenu(fileName = "XpGainData", menuName = "Moe/Data/XpGainData")]
    public class XpGainData : ScriptableObject
    {
        [field: SerializeField] public int defaultXpThreshold = 100;
        [field: SerializeField] public float xpDivider = 8f;
        [field: SerializeField] public int damageIncrease = 5;
        [field: SerializeField] public float attackSpeedDecrease = 0.05f;
        [field: SerializeField] public float minAttackSpeed = 0.1f;
        [field: SerializeField] public int maxDamage = 75;
        [field: SerializeField] public int maxLevel = 10;
    }
}