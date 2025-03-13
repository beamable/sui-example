using System;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Interfaces;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Player
{
    public class PlayerMelee : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private SpriteRenderer meleeWeaponSpriteRenderer;

        #endregion
        
        #region PRIVATE_VARIABLES

        private WeaponInstance _meleeWeapon;
        
        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        public void Init(WeaponInstance weapon)
        {
            _meleeWeapon = weapon;
            meleeWeaponSpriteRenderer.sprite = _meleeWeapon.Icon;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out BaseEnemy enemy)) return;
            
            enemy.TakeDamage(_meleeWeapon.MetaData.CurrentDamage, _meleeWeapon.InstanceId);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}