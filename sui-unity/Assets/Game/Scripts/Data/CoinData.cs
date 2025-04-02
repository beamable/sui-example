using UnityEngine;

namespace MoeBeam.Game.Scripts.Data
{
    [CreateAssetMenu(fileName = "CoinData", menuName = "Moe/Data/CoinData", order = 0)]
    public class CoinData : ScriptableObject
    {
        [SerializeField] private int maxCoinValue = 5;
        [field: SerializeField] public GameData.CoinType coinType = GameData.CoinType.Gold;
        [field: SerializeField] public AnimatorOverrideController coinAnimatorController;

        public int GetCoinValue()
        {
            return Random.Range(1, maxCoinValue);
        }
    }
}