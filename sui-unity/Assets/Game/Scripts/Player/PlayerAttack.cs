using System;
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

        #endregion

        #region PRIVATE_VARIABLES

        private float _nextMeleeAttackTime;
        private float _nextShootTime;
        private WeaponInstance _meleeWeapon;
        private WeaponInstance _rangedWeapon;
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
        }

        public void Init()
        {
            _playerAnimationController = GetComponent<PlayerAnimationController>();
            
            //Equip weapons
            _meleeWeapon = WeaponContentManager.Instance.GetOwnedMeleeWeapon();
            _rangedWeapon = WeaponContentManager.Instance.GetOwnedRangedWeapon();
            rangedWeaponSpriteRenderer.sprite = _rangedWeapon.Icon;
            
            playerMelee.Init(_meleeWeapon);
        }

        public void DeInitModules()
        {
            _inputReader.PrimaryAttackEvent -= PlayPrimaryAttackAnim;
            _inputReader.SecondaryAttackEvent -= PlaySecondaryAttack;
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void PlayPrimaryAttackAnim()
        {
            if(!GameManager.Instance.HasGameStarted) return;
            if (Time.time < _nextMeleeAttackTime) return;
            _nextMeleeAttackTime = Time.time + _meleeWeapon.MetaData.CurrentAttackSpeed;

            switch (_meleeWeapon.AttackType)
            {
                case GameData.AttackType.Thrust:
                    _playerAnimationController.PlayThrustAnimation();
                    break;
                case GameData.AttackType.Swing:
                    _playerAnimationController.PlaySwingAnimation();
                    break;
            }
        }

        private void PlaySecondaryAttack()
        {
            if(!GameManager.Instance.HasGameStarted) return;
            if (Time.time < _nextShootTime) return;
            
            _nextShootTime = Time.time + _rangedWeapon.MetaData.CurrentAttackSpeed;
            var bullet = GenericPoolManager.Instance.Get<Bullet>();
            bullet.transform.position = shootPoint.position;
            bullet.transform.rotation = shootPoint.rotation;
            bullet.Launch(_rangedWeapon);
        }

        #endregion


        
    }
}