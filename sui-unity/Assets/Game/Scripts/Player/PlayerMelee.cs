using System;
using DG.Tweening;
using MoeBeam.Game.Scripts.Data;
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
        private Material _material;
        
        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        public void Init(WeaponInstance weapon)
        {
            _meleeWeapon = weapon;
            meleeWeaponSpriteRenderer.sprite = _meleeWeapon.Icon;
            _material = new Material(meleeWeaponSpriteRenderer.material);
            
            EventCenter.Subscribe(GameData.OnMeleeLeveledUpEvent, OnMeleeLeveledUp);
        }

        private void OnMeleeLeveledUp(object _)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_material.DOFloat(1f, "_OutlineAlpha", 0f));
            sequence.Join(_material.DOFloat(7f, "_OutlineGlow", 0));
            sequence.Join(_material.DOFloat(35f, "_GlowGlobal", 0f));
            sequence.AppendInterval(1.5f);
            sequence.Append(_material.DOFloat(0f, "_OutlineAlpha", 1f));
            sequence.Join(_material.DOFloat(0f, "_OutlineGlow", 1f));
            sequence.Join(_material.DOFloat(0f, "_GlowGlobal", 1f));

            sequence.Play();
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