using System;
using DG.Tweening;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class UiDemoManager : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [Header("Loading Screen")]
        [SerializeField] private float fakeLoadingTime = 5f;
        [SerializeField] private float fadeTime = 1f;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private Slider loadingSlider;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES
        
        public event Action OnLoadingScreenFinished;

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            StartLoading();
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void StartLoading()
        {
            loadingCanvasGroup.alpha = 1;
            loadingSlider.DOValue(0f,0f);
            loadingSlider.DOValue(1f, fakeLoadingTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                loadingCanvasGroup.DOFade(0, fadeTime).OnComplete(() =>
                {
                    EventCenter.InvokeEvent(GameData.OnDemoLoadingScreenFinished);
                });
            });
        }

        #endregion


    }
}