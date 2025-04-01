using System;
using DG.Tweening;
using Game.Scripts.UI;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private AudioClip gameOverClip;

        #endregion

        #region PRIVATE_VARIABLES
        
        #endregion

        #region PUBLIC_VARIABLES
        
        public bool IsGamePaused { get; private set; }
        public bool HasGameStarted { get; private set; }
        
        public bool GameEnded { get; private set; }
        
        public int EnemiesKilled { get; private set; }

        #endregion

        #region UNITY_CALLS

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            EventCenter.Subscribe(GameData.OnDemoLoadingScreenFinishedEvent, OnLoadingScreenFinished);
            EventCenter.Subscribe(GameData.OnEnemyDiedEvent, EnemyKilled);
            EventCenter.Subscribe(GameData.OnPlayerDiedEvent, GameOver);
        }

        private void OnDisable()
        {
            EventCenter.Unsubscribe(GameData.OnDemoLoadingScreenFinishedEvent, OnLoadingScreenFinished);
            EventCenter.Unsubscribe(GameData.OnEnemyDiedEvent, EnemyKilled);
            EventCenter.Unsubscribe(GameData.OnPlayerDiedEvent, GameOver);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void OnLoadingScreenFinished(object obj)
        {
            HasGameStarted = true;
        }
        
        private void EnemyKilled(object _)
        {
            EnemiesKilled++;
            
            var tenEnemiesKilled = EnemiesKilled % 10 == 0;
            if (tenEnemiesKilled)
            {
                EventCenter.InvokeEvent(GameData.OnTenEnemiesKilledEvent);
            }
        }
        
        private void GameOver(object _)
        {
            GameEnded = true;
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() =>
            {
                AudioManager.Instance.StopMusic();
                Time.timeScale = 0.5f;
                AudioManager.Instance.PlaySfx(gameOverClip);
            });
            sequence.AppendInterval(2f);
            sequence.AppendCallback(() =>
            {
                Time.timeScale = 1f;
                EventCenter.InvokeEvent(GameData.OnPlayerDeathSequenceDoneEvent);
            });
        }
        
        #endregion

        
    }
}