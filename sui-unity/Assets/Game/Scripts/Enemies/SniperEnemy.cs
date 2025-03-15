using System.Collections;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace Game.Scripts.Enemies
{
    public class SniperEnemy : BaseEnemy
    {
        #region EXPOSED_VARIABLES
        
        [Header("Sniper")]
        [SerializeField] private Transform shootPoint;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        #endregion

        #region PUBLIC_METHODS

        protected override void Attack()
        {
            if(_distanceToPlayer >= enemyData.AttackRange) return;
            if(_attackCoroutine != null) return;
            _attackCoroutine = StartCoroutine(AttackRoutine());
            return;
            
            IEnumerator AttackRoutine()
            {
                if (_isAttacking || _isInjured || _isDead) yield break;
                if (Time.time < _nextAttackTime) yield break;
                _nextAttackTime = Time.time + enemyData.AttackCooldown;
                
                var bullet = GenericPoolManager.Instance.Get<SniperBullet>(shootPoint.position);
                bullet.transform.rotation = shootPoint.rotation;
                bullet.Launch(enemyData.AttackPower);
                
                _isAttacking = true;
                enemyAnimator.SetTrigger(AttackHash);
                yield return _attackWait;
                _isAttacking = false;
                _attackCoroutine = null;
            }
            
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}