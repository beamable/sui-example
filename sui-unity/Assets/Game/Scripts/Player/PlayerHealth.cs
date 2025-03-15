using System;
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
            _currentHealth -= (int)damage;
            _playerAnimationController.SetInjuredTrigger();
            EventCenter.InvokeEvent(GameData.OnPlayerInjuredEvent, _currentHealth);
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