using System;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES
        
        private LoadingScreenController _loadingScreenController;

        #endregion

        #region PUBLIC_VARIABLES
        
        public bool IsGamePaused { get; private set; }
        public bool HasGameStarted { get; private set; }

        #endregion

        #region UNITY_CALLS

        protected override void Awake()
        {
            base.Awake();
            _loadingScreenController = FindFirstObjectByType<LoadingScreenController>();
        }

        private void Start()
        {
            _loadingScreenController.OnLoadingScreenFinished += OnLoadingScreenFinished;
        }

        private void OnDisable()
        {
            _loadingScreenController.OnLoadingScreenFinished -= OnLoadingScreenFinished;
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void OnLoadingScreenFinished()
        {
            HasGameStarted = true;
        }

        #endregion

        
    }
}