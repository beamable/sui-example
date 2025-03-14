using System;
using Game.Scripts.UI;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES
        
        #endregion

        #region PUBLIC_VARIABLES
        
        public bool IsGamePaused { get; private set; }
        public bool HasGameStarted { get; private set; }

        #endregion

        #region UNITY_CALLS

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            EventCenter.Subscribe(GameData.OnDemoLoadingScreenFinished, OnLoadingScreenFinished);
        }

        private void OnDisable()
        {
            EventCenter.Unsubscribe(GameData.OnDemoLoadingScreenFinished, OnLoadingScreenFinished);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void OnLoadingScreenFinished(object obj)
        {
            HasGameStarted = true;
        }

        #endregion

        
    }
}