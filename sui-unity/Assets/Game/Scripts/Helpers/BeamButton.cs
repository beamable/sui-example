using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.Helpers
{
    [RequireComponent(typeof(Button))]
    public class BeamButton : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private TextMeshProUGUI buttonText;

        #endregion

        #region PRIVATE_VARIABLES
        
        private Button _uiButton;
        private string _originalText = string.Empty;

        #endregion

        #region PUBLIC_VARIABLES
        
        public Button ButtonCurrent => _uiButton;

        #endregion

        #region UNITY_CALLS

        private void Awake()
        {
            _uiButton = GetComponent<Button>();
        }

        private void Start()
        {
            _originalText = buttonText.text;
        }

        #endregion

        #region PUBLIC_METHODS
        
        public void AddListener(Action action)
        {
            _uiButton.onClick.AddListener(() => action());
        }
        
        public void RemoveListener(Action action)
        {
            _uiButton.onClick.RemoveListener(() => action());
        }
        
        public void SwitchText(bool toDefault, string newText = "")
        {
            buttonText.text = toDefault ? _originalText : newText;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion


    }
}