using System;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private AudioClip injuredClip;
        
        #endregion

        private int _currentHealth;
        private PlayerAnimationController _playerAnimationController;

        public void Init(PlayerAnimationController animController)
        {
            _currentHealth = maxHealth;
            _playerAnimationController = animController;
        }
        
        public void TakeDamage(float damage)
        {
            if(GameManager.Instance.GameEnded) return;
            _currentHealth -= (int)damage;
            _playerAnimationController.SetInjuredTrigger();
            EventCenter.InvokeEvent(GameData.OnPlayerInjuredEvent, _currentHealth);
            AudioManager.Instance.PlaySfx(injuredClip);
            
            //choose a random coinType
            var coinType = (GameData.CoinType)UnityEngine.Random.Range(0, 3);
            BeamInventoryManager.Instance.UpdateCurrency(coinType, true).Forget();
            
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }
        
        public void AddHealth(int health)
        {
            _currentHealth += health;
            if (_currentHealth > maxHealth)
            {
                _currentHealth = maxHealth;
            }
            EventCenter.InvokeEvent(GameData.OnPlayerInjuredEvent, _currentHealth);
        }
        
        private void Die()
        {
            EventCenter.InvokeEvent(GameData.OnPlayerDiedEvent);
        }
    }
}