using System;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Enemies
{
    public class EnemyAttacker : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        private bool _canAttack = false;
        private int _damageAmount = 0;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canAttack) return;
            if (other.CompareTag(GameData.PlayerTag) && other.TryGetComponent(out Player.PlayerHealth player))
            {
                player.TakeDamage(_damageAmount);
                _canAttack = false;
            }
        }

        #endregion

        #region PUBLIC_METHODS

        public void CanAttack(int damage)
        {
            _canAttack = true;
            _damageAmount = damage;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}