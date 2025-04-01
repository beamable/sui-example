using System;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Interfaces;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Player
{
    public class Bullet : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 2f;
        [FormerlySerializedAs("bulleetRenderer")] [SerializeField] private SpriteRenderer bulletRenderer;
        #endregion

        #region PRIVATE_VARIABLES
        
        private float _currentLifeTime = 0f;
        private bool _isLaunched = false;
        private WeaponInstance _currentWeapon;
        private Rigidbody2D _rigidbody2D;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void OnDisable()
        {
            _currentLifeTime = 0f;
            _isLaunched = false;
        }

        private void Update()
        {
            _currentLifeTime += Time.deltaTime;
            if (_currentLifeTime >= lifeTime)
            {
                GenericPoolManager.Instance.Return(this);
            }
        }

        private void FixedUpdate()
        {
            if (!_isLaunched) return;
            _rigidbody2D.linearVelocity = transform.up * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out BaseEnemy enemy)) return;
            
            enemy.TakeDamage(_currentWeapon.MetaData.CurrentDamage, _currentWeapon.InstanceId);
            GenericPoolManager.Instance.Return(this);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void Launch(WeaponInstance rangedWeapon)
        {
            _currentWeapon = rangedWeapon;
            bulletRenderer.sprite = _currentWeapon.Icon;
            _isLaunched = true;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}