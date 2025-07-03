using System;
using System.Runtime.InteropServices;
using System.Text;
using Beamable.Common;
using Beamable.Player;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.Managers
{
    public class UiWalletManager : MonoBehaviour
    {
        [Header("Account Info")] 
        [SerializeField] private TextMeshProUGUI aliasText;
        [SerializeField] private TextMeshProUGUI normalWalletText;
        [SerializeField] private TextMeshProUGUI walletAddressText;
        
        [Header("Buttons")] 
        [SerializeField] private Button getAddressBtn;
        [SerializeField] private Button attachBtn;
        [SerializeField] private Button openWalletBtn;
        [SerializeField] private Button suiScanBtn;
        [SerializeField] private Button suiScanStashedBtn;

        [Header("Currency Demo")] 
        [SerializeField] private RectTransform frameRectTransform;
        [SerializeField] private GameObject currencyDemoObject;
        [SerializeField] private TextMeshProUGUI beamInventoryText;
        [SerializeField] private TextMeshProUGUI beamStashedText;
        [SerializeField] private Vector2 posYOffsets = new Vector2(140, 30);
        [SerializeField] private Vector2 heightOffsets = new Vector2(420, 640);
        [SerializeField] private string beamCoinId;
        
        [Header("Continue Demo Btn")]
        [SerializeField] private RectTransform continueDemoBtnRect;
        [SerializeField] private Button continueDemoBtn;
        [SerializeField] private TextMeshProUGUI continueDemoBtnText;
        [SerializeField] private Vector2 continueBtnYOffsets = new Vector2(0, 0);
        [SerializeField] private string btnSkipText = "Skip To Game";
        [SerializeField] private string btnContinueText = "Continue Game";


        private bool _stashedWalletConnected = false;
        private string _walletAddress;
        private string messageToSign;
        private const string WalletName = "Slush";
        private const string WalletUrl = "https://my.slush.app";
        private const string SuiScanUrl = "https://suiscan.xyz/devnet/account/";
        private int stashedCoins = 0;
        
        private Promise<string> _challengePromise;

        #region Dlls

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PromptForPopupAndConnect();
        [DllImport("__Internal")]
        private static extern void ConnectWallet(string callbackMethod);

        [DllImport("__Internal")]
        private static extern void DisconnectWallet(string callbackMethod);

        [DllImport("__Internal")]
        private static extern void SignMessage(string message, string callbackMethod);
#else
        private static void PromptForPopupAndConnect()
        {
            Debug.Log("[Editor Stub] PromptForPopupAndConnect()");
        }
        private static void ConnectWallet(string callbackMethod)
        {
            Debug.Log($"[Editor Stub] ConnectWallet({callbackMethod})");
        }

        private static void DisconnectWallet(string callbackMethod)
        {
            Debug.Log($"[Editor Stub] DisconnectWallet({callbackMethod})");
        }

        private static void SignMessage(string message, string callbackMethod)
        {
            Debug.Log($"[Editor Stub] SignMessage({message}, {callbackMethod})");
        }
#endif

        #endregion

        private async void Start()
        {
            //PromptForPopupAndConnect();
            await UniTask.WaitUntil(() => BeamAccountManager.Instance.IsReady);
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            getAddressBtn.interactable = true;
            attachBtn.interactable = false;
            openWalletBtn.interactable = false;
            
            suiScanBtn.interactable = false;
            suiScanStashedBtn.interactable = false;
            
            frameRectTransform.DOAnchorPosY(posYOffsets.x, 0f);
            frameRectTransform.DOSizeDelta(new Vector2(frameRectTransform.sizeDelta.x, heightOffsets.x), 0f);
            continueDemoBtnRect.DOAnchorPosY(continueBtnYOffsets.x, 0f);
            continueDemoBtnText.text = btnSkipText;
            currencyDemoObject.SetActive(false);
            
            //Setup Account and buttons stats
            SetupAccount();
        }

        private void SetupAccount()
        {
            aliasText.text = "Alias: " + BeamAccountManager.Instance.CurrentAccount.Alias;
            normalWalletText.text = "Sui Wallet: " + BeamAccountManager.Instance.CurrentAccount.ExternalIdentities[0].userId;
            _stashedWalletConnected = BeamAccountManager.Instance.CurrentAccount.ExternalIdentities.Length > 1;
            if(!_stashedWalletConnected) return;
            walletAddressText.text = "Stashed Wallet: " + BeamAccountManager.Instance.CurrentAccount.ExternalIdentities[1].userId;
            StashedWalletSuccessSequence();
        }

        #region BTN_Clicks

        public void OnClickConnectWallet()
        {
            try
            {
                ConnectWallet(nameof(OnWalletConnected));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting wallet: {e.Message}");
            }
        }

        public void OnClickDisconnectWallet()
        {
            try
            {
                DisconnectWallet(nameof(OnWalletDisconnected));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disconnecting wallet: {e.Message}");
            }
        }

        public void OnClickOpenExternalWallet()
        {
            try
            {
                Application.OpenURL(WalletUrl);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error opening wallet: {e.Message}");
            }
        }
        
        public void OnClickAttachWallet()
        {
            try
            {
                BeamAccountManager.Instance.AddStashedExternalIdentity(_walletAddress, SolveChallenge).ContinueWith(
                    result =>
                    {
                        walletAddressText.text = $"Stashed Wallet: {_walletAddress}";
                        Debug.Log($"Wallet connected: {_walletAddress}");
                        
                        StashedWalletSuccessSequence();
                    });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to wallet: {e.Message}");
            }
        }

        private void StashedWalletSuccessSequence()
        {
            getAddressBtn.interactable = false;
            attachBtn.interactable = false;
            openWalletBtn.interactable = true;
            suiScanBtn.interactable = true;
            suiScanStashedBtn.interactable = true;
                        
            ExpandDemoFrame();
        }

        public async void OnClickAddToBeam()
        {
            var coin = BeamInventoryManager.Instance.GetCoinByType(GameData.CoinType.Beam);
            await BeamInventoryManager.Instance.UpdateCurrency(GameData.CoinType.Beam);
            beamInventoryText.text = $"{coin.Amount}";
        }

        public async void OnClickWithdraw()
        {
            var coin = BeamInventoryManager.Instance.GetCoinByType(GameData.CoinType.Beam);
            await BeamInventoryManager.Instance.UpdateCurrency(GameData.CoinType.Beam, true);
            beamInventoryText.text = $"{coin.Amount}";
            await BeamManager.SuiClient.Withdraw(beamCoinId, 1);
            stashedCoins += 1;
            beamStashedText.text = $"{stashedCoins}";
        }
        
        public void OnClickSuiScanNormal()
        {
            try
            {
                Application.OpenURL($"{SuiScanUrl}{BeamAccountManager.Instance.CurrentAccount.ExternalIdentities[0].userId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error opening SuiScan: {e.Message}");
            }
        }
        
        public void OnClickSuiScanStashed()
        {
            try
            {
                Application.OpenURL($"{SuiScanUrl}{_walletAddress}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error opening SuiScan: {e.Message}");
            }
        }

       
        #endregion

        #region Privat_Methods

        private async Promise<string> SolveChallenge(string challengeToken)
        {
            try
            {
                Debug.Log($"Signing a challenge: {challengeToken}");
                var parsedToken = BeamManager.BeamContext.Api.AuthService.ParseChallengeToken(challengeToken);
                if (parsedToken.challenge == null)
                {
                    Debug.LogError($"Failed to parse challenge token: {challengeToken}");
                    return Promise<string>.Failed(new Exception("Failed to parse challenge token")).ToString();
                }
                var challengeBytes = Convert.FromBase64String(parsedToken.challenge);
                var regularString = Encoding.UTF8.GetString(challengeBytes);

                var singed = await SignToSolveMessage(regularString);
                Debug.Log($"Solved the challenge: {singed}");
                return singed;
            }
            catch (Exception e)
            {
                return Promise<string>.Failed(e).ToString();
            }
        }
        
        private Promise<string> SignToSolveMessage(string message)
        {
            _challengePromise = new Promise<string>();
            try
            {
                SignMessage(message, nameof(OnMessageSigned));
                return _challengePromise;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error signing message: {e.Message}");
                return Promise<string>.Failed(e);
            }
        }
        
        private void ExpandDemoFrame()
        {
            continueDemoBtn.interactable = false;
            continueDemoBtnRect.DOAnchorPosY(continueBtnYOffsets.y, 0.5f).SetEase(Ease.Linear)
                .OnComplete(() =>
            {
                continueDemoBtn.interactable = true;
                continueDemoBtnText.text = btnContinueText;
            });
            
            frameRectTransform.DOAnchorPosY(posYOffsets.y, 0.5f).SetEase(Ease.Linear);
            frameRectTransform.DOSizeDelta(new Vector2(frameRectTransform.sizeDelta.x, heightOffsets.y),
                0.5f).SetEase(Ease.Linear).OnComplete(()=> currencyDemoObject.SetActive(true));
        }

        #endregion

        #region JS_Calls

        // Called by JS
        public void OnWalletConnected(string address)
        {
            walletAddressText.text = $"Slush: {address}";
            Debug.Log($"Wallet connected: {address}");

            if (!string.IsNullOrEmpty(address))
            {
                _walletAddress = address;
                
                getAddressBtn.interactable = false;
                attachBtn.interactable = true;
                openWalletBtn.interactable = false;
            }
        }

        public void OnWalletDisconnected(string _ = null)
        {
            walletAddressText.text = "No Stashed Wallet Attached";
            Debug.Log("Wallet disconnected");
        }

        public void OnMessageSigned(string signed)
        {
            _challengePromise?.CompleteSuccess(signed);
            Debug.Log($"Message signed: {signed}");
        }

        public void OnPopupResult(string message)
        {
            Debug.LogWarning($"Result from popup: {message}");
        }
        
        #endregion

        
    }
}
