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
        [SerializeField] private GameObject chooseWeaponsPanel;
        [SerializeField] private GameObject playPanel;
        
        [Header("Create User")]
        [SerializeField] private BeamButton createNewAccountBeamButton;
        [SerializeField] private TMP_InputField changeNameInputField;
        

        private bool _hasCreatedNewUser;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            PlayPanelStatus(false);
            createNewAccountPanel.SetActive(false);
            mainMenuPanel.SetActive(false);
            chooseWeaponsPanel.SetActive(false);
            beamInitTextObj.SetActive(true);
            await UniTask.WaitUntil(()=> BeamManager.IsReady);
            beamInitTextObj.SetActive(false);
            
            mainMenuPanel.SetActive(true);
            createNewAccountPanel.SetActive(true);
            
        }
        
        private void Update()
        {
            if(_hasCreatedNewUser) return;
            createNewAccountBeamButton.ButtonCurrent.interactable = changeNameInputField.text.Length >= 3;
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void PlayPanelStatus(bool status)
        {
            playPanel.SetActive(status);
        }
        
        public async void OnCreateNewAccount()
        {
            try
            {
                createNewAccountBeamButton.ButtonCurrent.interactable = false;
                createNewAccountBeamButton.SwitchText(false, "Creating...");
                await AccountManager.Instance.CreateNewAccount();
                await AccountManager.Instance.ChangeAlias(changeNameInputField.text);
                createNewAccountPanel.SetActive(false);
                chooseWeaponsPanel.SetActive(true);
                _hasCreatedNewUser = true;
            }
            catch (Exception e)
            {
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