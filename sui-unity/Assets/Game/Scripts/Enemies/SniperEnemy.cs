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
            base.Attack();
            
            var bullet = GenericPoolManager.Instance.Get<SniperBullet>(shootPoint.position);
            bullet.transform.rotation = shootPoint.rotation;
            bullet.Launch(enemyData.AttackPower);

            
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}