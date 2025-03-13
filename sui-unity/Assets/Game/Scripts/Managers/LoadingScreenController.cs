using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.Managers
{
    public class LoadingScreenController : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private float fakeLoadingTime = 5f;
        [SerializeField] private float fadeTime = 1f;
        [SerializeField] private CanvasGroup canvasGroup;
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
            canvasGroup.alpha = 1;
            loadingSlider.DOValue(0f,0f);
            loadingSlider.DOValue(1f, fakeLoadingTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                canvasGroup.DOFade(0, fadeTime).OnComplete(() =>
                {
                    OnLoadingScreenFinished?.Invoke();
                });
            });
        }

        #endregion


    }
}