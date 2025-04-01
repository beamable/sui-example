using MoeBeam.Game.Scripts.Data;
using UnityEditor.Animations;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Items
{
    public class CoinSelector : MonoBehaviour
    {
        [SerializeField] private Animator coinAnimator;
        [SerializeField] private AnimatorController goldController;
        [SerializeField] private AnimatorController starController;
        [SerializeField] private AnimatorController beamController;
        
        public void SelectCoinType(GameData.CoinType coinType)
        {
            switch (coinType)
            {
                case GameData.CoinType.Beam:
                    coinAnimator.runtimeAnimatorController = beamController;
                    break;
                case GameData.CoinType.Star:
                    coinAnimator.runtimeAnimatorController = starController;
                    break;
                case GameData.CoinType.Gold:
                    coinAnimator.runtimeAnimatorController = goldController;
                    break;
            }
        }

    }
}