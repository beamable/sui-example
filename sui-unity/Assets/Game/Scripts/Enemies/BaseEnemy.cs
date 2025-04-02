using System;
using System.Collections;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Helpers;
using MoeBeam.Game.Scripts.Interfaces;
using MoeBeam.Game.Scripts.Items;
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
        [SerializeField] protected Animator enemyAnimator;
        [SerializeField] protected EnemyAttacker enemyAttacker;
        [SerializeField] protected DamageFlasher damageFlasher;
        [SerializeField] private float miniBossMultiplier = 1.5f;
        #endregion

        #region PRIVATE_VARIABLES

        private long _lastWeaponInstanceId = 0;
        private float _currentHealth = 0f;
        protected float _nextAttackTime = 0f;
        protected float _distanceToPlayer = Mathf.Infinity;
        protected bool _isInjured = false;
        protected bool _isAttacking = false;
        protected bool _introAnimation = false;
        protected bool _canMove = true;
        protected bool _isDead = false;
        private Vector2 _moveDirection = Vector2.zero;
        
        private Coroutine _injuredCoroutine;
        private WaitForSeconds _injuredWait = new WaitForSeconds(1f);
        protected Coroutine _attackCoroutine;
        protected WaitForSeconds _attackWait = new WaitForSeconds(1f);
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
            _currentHealth = enemyData.MaxHealth;
            CurrentPlayer = FindFirstObjectByType<Player.Player>();
            Rb2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_isDead) return;
            OnUpdate();
        }

        private void FixedUpdate()
        {
            if(_isDead || Rb2D is null ) return;
            Rb2D.linearVelocity = _moveDirection * enemyData.MoveSpeed;
        }

        protected virtual void OnUpdate()
        {
            if(!GameManager.Instance.HasGameStarted) return;

            CalculateDistanceToPlayer();
            CheckCanMove();
            LookAtPlayer();
            Attack();
            MoveTowardsPlayer();
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void SetMiniBoss(bool miniBoss)
        {
            Vector3 newScale = miniBoss ? Vector3.one * miniBossMultiplier : Vector3.one; 
            var newHealth = miniBoss ? enemyData.MaxHealth * miniBossMultiplier : enemyData.MaxHealth;
            mainRenderer.transform.localScale = newScale;
            _currentHealth = newHealth;
        }
        
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
                damageFlasher.Flash();
                yield return _injuredWait;
                _isInjured = false;
                _injuredCoroutine = null;
                if (_currentHealth < 0)
                {
                    _currentHealth = 0;
                    _isDead = true;
                    Die();
                }
            }
        }

        #endregion

        #region PRIVATE_METHODS
        
        private void CalculateDistanceToPlayer()
        {
            _distanceToPlayer = Vector2.Distance(transform.position, CurrentPlayer.transform.position);
        }

        private void CheckCanMove()
        {
            _canMove = !(_distanceToPlayer <= enemyData.AttackRange) && !_isAttacking && !_isInjured 
                       && !_isDead && !_introAnimation;
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
        
        private void FlipOnYAxis(bool reset = false)
        {
            if (reset)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            transform.localScale = transform.position.y < CurrentPlayer.transform.position.y ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        }
        
        private void FlipOnXAxis(bool reset = false)
        {
            if (reset)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            transform.localScale = transform.position.x < CurrentPlayer.transform.position.x ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        }
        
        protected virtual void Attack()
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
                _isAttacking = true;
                enemyAnimator.SetTrigger(AttackHash);
                enemyAttacker.CanAttack(enemyData.AttackPower);
                yield return _attackWait;
                _isAttacking = false;
                _attackCoroutine = null;
            }
        }
        
        protected virtual void Die(int coinAmount = 1)
        {
            var deathData = new EnemyKilledData(enemyData.XpValue, _lastWeaponInstanceId);
            enemyAnimator.SetTrigger(DeathHash);
            SpawnRandomCoin(coinAmount);

            EventCenter.InvokeEvent(GameData.OnEnemyKillRewardEvent, deathData);
            EventCenter.InvokeEvent(GameData.OnEnemyDiedEvent, this);
        }

        private void SpawnRandomCoin(int coinAmount)
        {
            for (int i = 0; i < coinAmount; i++)
            {
                var randomOffset = UnityEngine.Random.insideUnitCircle;
                var randomPosOffset = new Vector3(randomOffset.x + transform.position.x,
                    randomOffset.y + transform.position.y, 0);
                var coin = GenericPoolManager.Instance.Get<CoinSelector>(randomPosOffset);
                coin.SelectCoinType();
            }
        }

        #endregion

        
    }
}