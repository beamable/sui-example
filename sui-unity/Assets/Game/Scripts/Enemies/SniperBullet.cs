using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Enemies
{
    public class SniperBullet : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private SpriteRenderer bulleetRenderer;
        [SerializeField] private Sprite bulletIcon;
        #endregion

        #region PRIVATE_VARIABLES
        
        private int _currentDamage;
        private float _currentLifeTime = 0f;
        private bool _isLaunched = false;
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
            var s = transform.right * this.speed;
            _rigidbody2D.linearVelocity = transform.right * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Player.PlayerHealth enemy)) return;
            
            enemy.TakeDamage(_currentDamage);
            GenericPoolManager.Instance.Return(this);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void Launch(int damage)
        {
            _currentDamage = damage;
            bulleetRenderer.sprite = bulletIcon;
            _isLaunched = true;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion
    }
}