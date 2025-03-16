﻿using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoeBeam.Game.Input;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private PlayerMelee playerMelee;
        
        [Header("Attack Points")] 
        [SerializeField] private SpriteRenderer rangedWeaponSpriteRenderer;
        [SerializeField] private Transform shootPoint;
        
        [Header("Clips")]
        [SerializeField] private AudioClip swingClip;
        [SerializeField] private AudioClip thrustClip;
        [SerializeField] private AudioClip secondaryAttackClip;

        #endregion

        #region PRIVATE_VARIABLES

        private float _nextMeleeAttackTime;
        private float _nextShootTime;
        private WeaponInstance _meleeWeapon;
        private WeaponInstance _rangedWeapon;
        private Material _rangedWeaponMat;
        private InputReader _inputReader;
        private PlayerAnimationController _playerAnimationController;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        public void InitModules(InputReader reader)
        {
            _inputReader = reader;
            _inputReader.PrimaryAttackEvent += PlayPrimaryAttackAnim;
            _inputReader.SecondaryAttackEvent += PlaySecondaryAttack;
            
            EventCenter.Subscribe(GameData.OnRangedLeveledUpEvent, OnRangedLeveledUp);
        }

        public async UniTask Init()
        {
            _playerAnimationController = GetComponent<PlayerAnimationController>();
            
            //Equip weapons
            await UniTask.WaitUntil(() => WeaponContentManager.Instance.GetOwnedMeleeWeapon() != null);
            await UniTask.WaitUntil(() => WeaponContentManager.Instance.GetOwnedRangedWeapon() != null);
            _meleeWeapon = WeaponContentManager.Instance.GetOwnedMeleeWeapon();
            _rangedWeapon = WeaponContentManager.Instance.GetOwnedRangedWeapon();
            rangedWeaponSpriteRenderer.sprite = _rangedWeapon.Icon;
            
            _rangedWeaponMat = new Material(rangedWeaponSpriteRenderer.material);
            playerMelee.Init(_meleeWeapon);
        }

        public void DeInitModules()
        {
            _inputReader.PrimaryAttackEvent -= PlayPrimaryAttackAnim;
            _inputReader.SecondaryAttackEvent -= PlaySecondaryAttack;
            EventCenter.Unsubscribe(GameData.OnRangedLeveledUpEvent, OnRangedLeveledUp);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void PlayPrimaryAttackAnim()
        {
            if(!GameManager.Instance.HasGameStarted || GameManager.Instance.GameEnded) return;
            if (Time.time < _nextMeleeAttackTime) return;
            _nextMeleeAttackTime = Time.time + _meleeWeapon.MetaData.CurrentAttackSpeed;

            switch (_meleeWeapon.AttackType)
            {
                case GameData.AttackType.Thrust:
                    AudioManager.Instance.PlaySfx(thrustClip);
                    _playerAnimationController.PlayThrustAnimation();
                    break;
                case GameData.AttackType.Swing:
                    AudioManager.Instance.PlaySfx(swingClip);
                    _playerAnimationController.PlaySwingAnimation();
                    break;
            }
        }

        private void PlaySecondaryAttack()
        {
            if(!GameManager.Instance.HasGameStarted || GameManager.Instance.GameEnded) return;
            if (Time.time < _nextShootTime) return;
            
            _nextShootTime = Time.time + _rangedWeapon.MetaData.CurrentAttackSpeed;
            var bullet = GenericPoolManager.Instance.Get<Bullet>(shootPoint.position);
            bullet.transform.rotation = shootPoint.rotation;
            bullet.Launch(_rangedWeapon);
            AudioManager.Instance.PlaySfx(secondaryAttackClip);
        }
        
        private void OnRangedLeveledUp(object _)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_rangedWeaponMat.DOFloat(1f, "_OutlineAlpha", 0f));
            sequence.Join(_rangedWeaponMat.DOFloat(7f, "_OutlineGlow", 1f));
            sequence.AppendInterval(0.5f);
            sequence.Append(_rangedWeaponMat.DOFloat(0f, "_OutlineAlpha", 1f));
            sequence.Join(_rangedWeaponMat.DOFloat(0f, "_OutlineGlow", 1f));
            sequence.Play();
        }

        #endregion


        
    }
}