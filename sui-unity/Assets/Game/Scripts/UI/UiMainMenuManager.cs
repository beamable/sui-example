using System;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Helpers;
using MoeBeam.Game.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.UI
{
    public class UiMainMenuManager : GenericSingleton<UiMainMenuManager>
    {
        #region EXPOSED_VARIABLES
        
        [Header("Panels")]
        [SerializeField] private GameObject beamInitTextObj;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject createNewAccountPanel;
        [SerializeField] private GameObject stashedWalletPanel;
        [SerializeField] private GameObject chooseWeaponsPanel;
        [SerializeField] private GameObject weaponsContainer;
        [SerializeField] private GameObject playPanel;
        
        [Header("Create User")]
        [SerializeField] private BeamButton createNewAccountBeamButton;
        [SerializeField] private TMP_InputField changeNameInputField;

        [Header("Weapon Chooser")] 
        [SerializeField] private TextMeshProUGUI weaponMintingText;
        
        [Header("Final ID")]
        [SerializeField] private GameObject finalIdContainer;
        [SerializeField] private TextMeshProUGUI aliasText;
        [SerializeField] private TextMeshProUGUI gamerTagText;
        [SerializeField] private TextMeshProUGUI externalIdText;
        

        private bool _hasCreatedNewUser;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES
        
        public bool PlayerHasWallet { get; private set; }

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            PlayPanelStatus(false);
            createNewAccountPanel.SetActive(false);
            mainMenuPanel.SetActive(false);
            chooseWeaponsPanel.SetActive(false);
            stashedWalletPanel.SetActive(false);
            beamInitTextObj.SetActive(true);
            
            await UniTask.WaitUntil(()=> BeamAccountManager.Instance.IsReady);
            beamInitTextObj.SetActive(false);
            
            mainMenuPanel.SetActive(true);
            PlayerHasWallet = BeamAccountManager.Instance.CurrentAccount.ExternalIdentities.Length > 0;
            createNewAccountPanel.SetActive(!PlayerHasWallet);
            stashedWalletPanel.SetActive(PlayerHasWallet);

        }
        
        private void Update()
        {
            if(_hasCreatedNewUser || !createNewAccountBeamButton.gameObject.activeInHierarchy) return;
            createNewAccountBeamButton.ButtonCurrent.interactable = changeNameInputField.text.Length >= 3;
        }

        #endregion

        #region PUBLIC_METHODS
        
        public async UniTask SetFinalId()
        {
            weaponsContainer.SetActive(false);
            aliasText.text = "User: " + BeamAccountManager.Instance.CurrentAccount.Alias;
            gamerTagText.text = "Tag: " + BeamAccountManager.Instance.CurrentAccount.GamerTag.ToString();
            externalIdText.text = "Wallet: " + BeamAccountManager.Instance.CurrentAccount.ExternalIdentities[0].userId;
            finalIdContainer.SetActive(true);
            weaponMintingText.gameObject.SetActive(true);
            await UniTask.WaitUntil(()=> BeamWeaponContentManager.Instance.GetOwnedWeaponsCount() > 1);
            weaponMintingText.gameObject.SetActive(false);
            PlayPanelStatus(true);
        }
        public void PlayPanelStatus(bool status)
        {
            playPanel.SetActive(status);
        }
        
        public async void OnCreateNewAccount()
        {
            try
            {
                _hasCreatedNewUser = true;
                createNewAccountBeamButton.ButtonCurrent.interactable = false;
                createNewAccountBeamButton.SwitchText(false, "Creating External ID...");
                await BeamAccountManager.Instance.CreateNewAccount();
                await BeamAccountManager.Instance.ChangeAlias(changeNameInputField.text);
                createNewAccountPanel.SetActive(false);
                stashedWalletPanel.SetActive(true);
               //chooseWeaponsPanel.SetActive(true);
                weaponsContainer.SetActive(true);
                finalIdContainer.SetActive(false);
            }
            catch (Exception e)
            {
                _hasCreatedNewUser = false;
                createNewAccountBeamButton.ButtonCurrent.interactable = true;
                createNewAccountBeamButton.SwitchText(true);
                Debug.LogError($"Create New Account error: {e.Message}");
            }
        }

        public void OnLoadDemo()
        {
            SceneController.Instance.LoadScene(SceneController.ScenesEnum.Game);
        }
        
        #endregion

        #region PRIVATE_METHODS


        #endregion

        
    }
}