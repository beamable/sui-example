using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoeBeam.Game.Scripts.Items
{
    public class CoinSelector : MonoBehaviour
    {
        [SerializeField] private Animator coinAnimator;
        [SerializeField] private Collider2D coinCollider;
        [SerializeField] private CoinData goldData;
        [SerializeField] private CoinData starData;
        [SerializeField] private CoinData beamData;
        
        public CoinData CurrentCoin { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameData.PlayerTag)) return;

            BeamInventoryManager.Instance.UpdateCurrency(CurrentCoin.coinType).Forget();
            GenericPoolManager.Instance.Return(this);
        }

        public void SelectCoinType()
        {
            //Choose a random coin type
            var coinType = (GameData.CoinType) Random.Range(0, 3);
            coinCollider.enabled = false;
            Invoke(nameof(EnableCoinCollider), 0.5f);
            switch (coinType)
            {
                case GameData.CoinType.Beam:
                    CurrentCoin = beamData;
                    coinAnimator.runtimeAnimatorController = beamData.coinAnimatorController;
                    break;
                case GameData.CoinType.Star:
                    CurrentCoin = starData;
                    coinAnimator.runtimeAnimatorController = starData.coinAnimatorController;
                    break;
                case GameData.CoinType.Gold:
                    CurrentCoin = goldData;
                    coinAnimator.runtimeAnimatorController = goldData.coinAnimatorController;
                    break;
                
            }
        }

        private void EnableCoinCollider()
        {
            coinCollider.enabled = true;
        }
    }
}