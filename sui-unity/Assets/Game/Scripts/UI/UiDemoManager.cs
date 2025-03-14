using System;
using DG.Tweening;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

        [Header("Other")] 
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        
        [Header("Death Screen")]
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button deathMarketButton;
        
        [Header("Win Screen")]
        [SerializeField] private GameObject winScreen;
        [FormerlySerializedAs("winRestartButton")] [SerializeField] private Button winQuitButton;
        [SerializeField] private Button marketButton;
        
        #endregion

        #region PRIVATE_VARIABLES
        
        private float _trailTimer;
        private const string SuiUrl = "https://suiscan.xyz/devnet/account/";

        #endregion

        #region PUBLIC_VARIABLES
        
        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            StartLoading();
            healthBar.fillAmount = 1f;
            healthBarTrail.fillAmount = 1f;
            
            SetPlayerIcons();
            
            EventCenter.Subscribe(GameData.OnPlayerInjuredEvent, UpdateHealthBar);
            EventCenter.Subscribe(GameData.OnEnemyDiedEvent, UpdateEnemiesKilled);
            EventCenter.Subscribe(GameData.OnPlayerDiedEvent, OnPlayerDied);
            EventCenter.Subscribe(GameData.OnBossDiedEvent, OnBossDied);
        }
        

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        
        private void OpenExternalLink()
        {
            var url = SuiUrl + AccountManager.Instance.CurrentAccount.ExternalIdentities[0].userId;
            Application.OpenURL("https://www.google.com");
        }

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
        
        private void UpdateEnemiesKilled(object obj)
        {
            enemiesKilledText.text = $"{GameManager.Instance.EnemiesKilled}";
        }

        private void OnPlayerDied(object _)
        {
            deathScreen.SetActive(true);
            restartButton.onClick.AddListener(OnRestart);
            quitButton.onClick.AddListener(OnQuit);
            deathMarketButton.onClick.AddListener(OpenExternalLink);
        }
        
        private void OnBossDied(object obj)
        {
            winScreen.SetActive(true);
            winQuitButton.onClick.AddListener(OnQuit);
            marketButton.onClick.AddListener(OpenExternalLink);
        }
        
        private void OnRestart()
        {
            SceneController.Instance.LoadScene(SceneController.ScenesEnum.Game);
        }

        private void OnQuit()
        {
            SceneController.Instance.LoadScene(SceneController.ScenesEnum.MainMenu);
        }

        #endregion


    }
}