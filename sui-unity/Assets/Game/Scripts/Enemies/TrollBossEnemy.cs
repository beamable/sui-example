using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Managers;

namespace Game.Scripts.Enemies
{
    public class TrollBossEnemy : BaseEnemy
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS

        protected override void Die()
        {
            base.Die();
            EventCenter.InvokeEvent(GameData.OnBossDiedEvent);
        }

        #endregion

        
    }
}