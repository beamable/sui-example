using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace Game.Scripts.Enemies
{
    public class TrollBossEnemy : BaseEnemy
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private AudioClip trollRoarClip;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS
        
        private void Start()
        {
            _introAnimation = true;
            AudioManager.Instance.PlaySfx(trollRoarClip);
            Invoke(nameof(TurnOffIntro), 1.5f);
        }

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        protected override void Die(int cointAmount = 1)
        {
            base.Die(3);
            EventCenter.InvokeEvent(GameData.OnBossDiedEvent);
        }
        
        private void TurnOffIntro()
        {
            _introAnimation = false;
        }

        #endregion

        
    }
}