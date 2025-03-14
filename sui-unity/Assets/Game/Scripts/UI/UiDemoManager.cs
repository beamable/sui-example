using System;
using DG.Tweening;
using MoeBeam.Game.Scripts.Beam;
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
        
        [Header("Player Health")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image healthBarTrail;
        [SerializeField] private float drainSpeed = 0.25f;
        [SerializeField] private float trailDelay = 0.4f;

        [Header("Player Icons")]
        [SerializeField] private Image meleeWeapon;
        [SerializeField] private Image rangedWeapon;


        #endregion

        #region PRIVATE_VARIABLES
        
        private float _trailTimer;

        #endregion

        #region PUBLIC_VARIABLES
        
        public event Action OnLoadingScreenFinished;

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            StartLoading();
            healthBar.fillAmount = 1f;
            healthBarTrail.fillAmount = 1f;
            
            SetPlayerIcons();
            
            EventCenter.Subscribe(GameData.OnPlayerInjuredEvent, UpdateHealthBar);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        private void SetPlayerIcons()
        {
            var melee = WeaponContentManager.Instance.GetOwnedMeleeWeapon().Icon;
            var ranged = WeaponContentManager.Instance.GetOwnedRangedWeapon().Icon;
            meleeWeapon.sprite = melee;
            rangedWeapon.sprite = ranged;
        }
        
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
        
        private void UpdateHealthBar(object currentHealth)
        {
            var ratio = (int)currentHealth / 100f;
            var sequence = DOTween.Sequence();
            sequence.Append(healthBar.DOFillAmount(ratio, drainSpeed)).SetEase(Ease.InOutSine);
            sequence.AppendInterval(trailDelay);
            sequence.Append(healthBarTrail.DOFillAmount(ratio, drainSpeed)).SetEase(Ease.InOutSine);
            
            sequence.Play();
        }

        #endregion


    }
}