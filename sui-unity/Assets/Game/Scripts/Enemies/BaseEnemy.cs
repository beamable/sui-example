using System;
using System.Collections;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Interfaces;
using MoeBeam.Game.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Enemies
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [SerializeField] protected EnemyData enemyData;
        [SerializeField] private SpriteRenderer mainRenderer;
        [SerializeField] private Animator enemyAnimator;
        [SerializeField] private EnemyAttacker enemyAttacker;
        #endregion

        #region PRIVATE_VARIABLES

        private long _lastWeaponInstanceId = 0;
        private float _currentHealth = 0f;
        private float _miniBossMultiplier = 1f;
        private float _nextAttackTime = 0f;
        protected float _distanceToPlayer = Mathf.Infinity;
        protected bool _isInjured = false;
        protected bool _isAttacking = false;
        protected bool _canMove = true;
        protected bool _isDead = false;
        private Vector2 _moveDirection = Vector2.zero;
        
        private Coroutine _injuredCoroutine;
        private WaitForSeconds _injuredWait = new WaitForSeconds(0.5f);
        protected EnemyData.EnemyAttackType CurrentAttackType => enemyData.AttackType;
        protected Player.Player CurrentPlayer;
        protected Rigidbody2D Rb2D;
        
        protected static readonly int IdleHash = Animator.StringToHash("idle");
        protected static readonly int MoveHash = Animator.StringToHash("move");
        protected static readonly int AttackHash = Animator.StringToHash("attack");
        protected static readonly int InjuredHash = Animator.StringToHash("injured");
        protected static readonly int DeathHash = Animator.StringToHash("death");
        
        #endregion

        #region PUBLIC_VARIABLES
        
        public EnemyData EnemyData => enemyData;

        #endregion

        #region UNITY_CALLS

        public virtual void OnInit()
        {
            _miniBossMultiplier = enemyData.IsMiniBoss ? 2f : 1f;
            mainRenderer.transform.localScale *= _miniBossMultiplier;
            _currentHealth = enemyData.MaxHealth * _miniBossMultiplier;
            CurrentPlayer = FindFirstObjectByType<Player.Player>();
            Rb2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            OnUpdate();
        }

        private void FixedUpdate()
        {
            Rb2D.linearVelocity = _moveDirection * enemyData.MoveSpeed;
        }

        protected virtual void OnUpdate()
        {
            if(!GameManager.Instance.HasGameStarted) return;

            CalculateDistanceToPlayer();
            CheckCanMove();
            LookAtPlayer();
            MoveTowardsPlayer();
        }

        #endregion

        #region PUBLIC_METHODS
        
        public virtual void TakeDamage(float damage, long instanceId = 0)
        {
            if (_isDead || _isInjured || _injuredCoroutine != null) return;
            _injuredCoroutine = StartCoroutine(InjuredRoutine());
            return;

            IEnumerator InjuredRoutine()
            {
                _isInjured = true;
                enemyAnimator.SetTrigger(InjuredHash);
                _lastWeaponInstanceId = instanceId;
                _currentHealth -= damage;
                if (_currentHealth <= 0)
                {
                    Die();
                }
                yield return _injuredWait;
                _isInjured = false;
                _injuredCoroutine = null;
            }
        }

        #endregion

        #region PRIVATE_METHODS
        
        private void CalculateDistanceToPlayer()
        {
            if (CurrentAttackType == EnemyData.EnemyAttackType.None) return;
            _distanceToPlayer = Vector2.Distance(transform.position, CurrentPlayer.transform.position);
        }

        private void CheckCanMove()
        {
            _canMove = !(_distanceToPlayer <= enemyData.AttackRange) || _isAttacking || _isInjured || _isDead;
            enemyAnimator.SetBool(MoveHash, _canMove);
        }
        
        private void MoveTowardsPlayer()
        {
            if (!_canMove)
            {
                _moveDirection = Vector2.zero;
                return;
            }
            _moveDirection = (CurrentPlayer.transform.position - transform.position).normalized;
        }
        
        private void LookAtPlayer()
        {
            var direction = CurrentPlayer.transform.position - transform.position;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        protected virtual void Attack()
        {
            if (_isAttacking || _isInjured || _isDead) return;
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + enemyData.AttackCooldown;
            enemyAttacker.CanAttack(enemyData.AttackPower);
            _isAttacking = true;
            enemyAnimator.SetTrigger(AttackHash);
        }
        
        protected virtual void Die()
        {
            _isDead = true;
            var deathData = new EnemyKilledData(enemyData.XpValue, _lastWeaponInstanceId);
            EventCenter.InvokeEvent(GameData.OnEnemyKillReward, deathData);
            //TODO: Implement death animation
            enemyAnimator.SetTrigger(DeathHash);
            //TODO: Implement death sound
            EventCenter.InvokeEvent(GameData.EnemyDiedEvent, this);
            //TODO: Implement pooling system
        }

        #endregion
        
    }
}