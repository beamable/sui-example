using Beamable.Avatars;
using Beamable.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoeBeam.Game.Scripts.Beam
{
    public class BeamCurrentAccountInfo : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI gamerTagText;
        [SerializeField] private TextMeshProUGUI emailText;
        [SerializeField] private Image avatar;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void OnEnable()
        {
            BeamAccountManager.OnSetCurrentAccount += SetAccountInfo;
        }

        private void OnDisable()
        {
            BeamAccountManager.OnSetCurrentAccount -= SetAccountInfo;
        }

        #endregion
        
        private void SetAccountInfo(PlayerAccount account)
        {
            nameText.text = account?.Alias ?? "Anonymous";
            gamerTagText.text = account?.GamerTag.ToString() ?? "0";
            emailText.text = account?.Email ?? "";
            avatar.sprite = account == null ? avatar.sprite = null : AvatarConfiguration.Instance.GetAvatarSprite(account.Avatar);
        }

    }
}