using System;
using System.Collections.Generic;
using DG.Tweening;
using MoeBeam.Game.Input;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Items;
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

        [Header("Weapons")]
        [SerializeField] private Image meleeWeapon;
        [SerializeField] private Image rangedWeapon;
        [SerializeField] private TextMeshProUGUI meleeWeaponLevel;
        [SerializeField] private TextMeshProUGUI rangedWeaponLevel;
        
        [Header("Death Screen")]
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private Button restartButton;
        [SerializeField] private CanvasGroup restartCanvasGroup;
        [SerializeField] private Button quitButton;
        [SerializeField] private CanvasGroup quitCanvasGroup;
        [SerializeField] private Button deathMarketButton;
        [SerializeField] private CanvasGroup deathMarketCanvasGroup;
        
        [Header("Win Screen")]
        [SerializeField] private GameObject winScreen;
        [FormerlySerializedAs("winRestartButton")] [SerializeField] private Button winQuitButton;
        [SerializeField] private Button marketButton;

        [Header("Coins")] 
        [SerializeField] private TextMeshProUGUI beamCoinText;
        [SerializeField] private TextMeshProUGUI starCoinText;
        [SerializeField] private TextMeshProUGUI goldCoinText;
        
        [Header("Other")] 
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private AudioClip deathLoopMusic;
        [SerializeField] private UiDemoWeaponShower uiWeaponShower;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private InputReader inputReader;
        
        #endregion

        #region PRIVATE_VARIABLES

        private bool _settingPanelOpened = false;
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

            //Set the player coins texts
            foreach (var coin in BeamInventoryManager.Instance.PlayerCoins)
            {
                switch (coin.CoinType)
                {
                    case GameData.CoinType.Beam:
                        beamCoinText.text = coin.Amount.ToString();
                        break;
                    case GameData.CoinType.Star:
                        starCoinText.text = coin.Amount.ToString();
                        break;
                    case GameData.CoinType.Gold:
                        goldCoinText.text = coin.Amount.ToString();
                        break;
                }
            }

            inputReader.EscapePressedEvent += OnEscapeKeyPressed;

        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            EventCenter.Unsubscribe(GameData.OnPlayerInjuredEvent, UpdateHealthBar);
            EventCenter.Unsubscribe(GameData.OnEnemyDiedEvent, UpdateEnemiesKilled);
            EventCenter.Unsubscribe(GameData.OnPlayerDeathSequenceDoneEvent, OnPlayerDied);
            EventCenter.Unsubscribe(GameData.OnBossDiedEvent, OnBossDied);
            EventCenter.Unsubscribe(GameData.OnMeleeLeveledUpEvent, OnWeaponLeveledUp);
            EventCenter.Unsubscribe(GameData.OnRangedLeveledUpEvent, OnWeaponLeveledUp);
            EventCenter.Unsubscribe(GameData.OnCoinCollectedEvent, OnCoinCollected);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void OnOpenSettings(bool open)
        {
            _settingPanelOpened = open;
            settingsPanel.SetActive(open);
            Time.timeScale = open ? 0f : 1f;
            ActivateUiWeaponCards(open);
        }
        
        public void OpenExternalLink()
        {
            var url = SuiUrl + BeamAccountManager.Instance.CurrentAccount.ExternalIdentities[0].userId;
            Application.OpenURL(url);
        }
        
        public void OnRestart()
        {
            EventCenter.ResetEventCenter();
            SceneController.Instance.LoadScene(SceneController.ScenesEnum.Game);
        }

        public void OnQuit()
        {
            EventCenter.ResetEventCenter();
            SceneController.Instance.LoadScene(SceneController.ScenesEnum.MainMenu);
        }

        #endregion

        #region PRIVATE_METHODS
        
        private void SetPlayerIcons()
        {
            var melee = BeamWeaponContentManager.Instance.GetOwnedMeleeWeapon().Icon;
            var ranged = BeamWeaponContentManager.Instance.GetOwnedRangedWeapon().Icon;
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
                    loadingCanvasGroup.gameObject.SetActive(false);
                    EventCenter.InvokeEvent(GameData.OnDemoLoadingScreenFinishedEvent);
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
        
        private void ActivateUiWeaponCards(bool active = true)
        {
            uiWeaponShower.gameObject.SetActive(active);
            uiWeaponShower.ShowWeaponCard(active);
        }
        
        private void SubscribeToEvents()
        {
            EventCenter.Subscribe(GameData.OnPlayerInjuredEvent, UpdateHealthBar);
            EventCenter.Subscribe(GameData.OnEnemyDiedEvent, UpdateEnemiesKilled);
            EventCenter.Subscribe(GameData.OnPlayerDeathSequenceDoneEvent, OnPlayerDied);
            EventCenter.Subscribe(GameData.OnBossDiedEvent, OnBossDied);
            EventCenter.Subscribe(GameData.OnMeleeLeveledUpEvent, OnWeaponLeveledUp);
            EventCenter.Subscribe(GameData.OnRangedLeveledUpEvent, OnWeaponLeveledUp);
            EventCenter.Subscribe(GameData.OnCoinCollectedEvent, OnCoinCollected);
        }

        private void OnCoinCollected(object obj)
        {
            if (obj is not Dictionary<PlayerCoin, bool> data) return;

            foreach (var pair in data)
            {
                var color = pair.Value ? Color.red : Color.green;
                switch (pair.Key.CoinType)
                {
                    case GameData.CoinType.Star:
                        TextLevelUpAnimation(starCoinText, pair.Key.Amount.ToString(), 1.3f, Ease.Flash, color);
                        break;
                    case GameData.CoinType.Beam:
                        TextLevelUpAnimation(beamCoinText, pair.Key.Amount.ToString(), 1.3f, Ease.Flash, color);
                        break;
                    case GameData.CoinType.Gold:
                        TextLevelUpAnimation(goldCoinText, pair.Key.Amount.ToString(), 1.3f, Ease.Flash, color);
                        break;
                }
            }
        }

        private void OnWeaponLeveledUp(object obj)
        {
            if(obj is not WeaponInstance weapon) return;
            if(weapon.AttackType != GameData.AttackType.Shoot)
                meleeWeaponLevel.text = weapon.MetaData.Level.ToString();
            else
                rangedWeaponLevel.text = weapon.MetaData.Level.ToString();
        }

        private void TextLevelUpAnimation(TextMeshProUGUI tmp, string newText, float endValue = 2f, Ease easeType = Ease.InElastic, Color color = default(Color))
        {
            var sequence = DOTween.Sequence();
            tmp.text = newText;
            sequence.Append(tmp.transform.DOScale(endValue, 0.25f)).SetEase(easeType);
            sequence.Join(tmp.DOColor(color, trailDelay));
            sequence.AppendInterval(0.5f);
            sequence.Append(tmp.transform.DOScale(1f, 0.25f)).SetEase(easeType);
            sequence.Join(tmp.DOColor(Color.white, trailDelay));
            sequence.Play();
        } 


        private void OnPlayerDied(object _)
        {
            deathScreen.SetActive(true);
            ActivateUiWeaponCards();
            AudioManager.Instance.PlayMusic(deathLoopMusic);
            var restartY = restartButton.transform.position.y;
            var quitY = quitButton.transform.position.y;
            var marketY = deathMarketButton.transform.position.y;
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(()=>
            {
                restartCanvasGroup.DOFade(0, 1f);
                quitCanvasGroup.DOFade(0, 1f);
                deathMarketCanvasGroup.DOFade(0, 1f);
            });
            sequence.AppendCallback(()=>
            {
                restartButton.transform.DOMoveY(restartY + 50f, 0f);
                quitButton.transform.DOMoveY(quitY + 50f, 0f);
                deathMarketButton.transform.DOMoveY(marketY + 50f, 0f);
            });
            sequence.AppendInterval(1f);
            sequence.AppendCallback(()=>
            {
                restartCanvasGroup.DOFade(1f, 1f);
                quitCanvasGroup.DOFade(1f, 1f);
                deathMarketCanvasGroup.DOFade(1f, 1f);
            });
            sequence.Join(restartButton.transform.DOMoveY(restartY, 1f));
            sequence.Join(quitButton.transform.DOMoveY(quitY, 1f));
            sequence.Join(deathMarketButton.transform.DOMoveY(marketY, 1f));
            sequence.Play();
        }
        
        private void OnBossDied(object obj)
        {
            winScreen.SetActive(true);
            ActivateUiWeaponCards();
        }
        
        private void OnEscapeKeyPressed()
        {
            OnOpenSettings(!_settingPanelOpened);
        }

        #endregion


    }
}