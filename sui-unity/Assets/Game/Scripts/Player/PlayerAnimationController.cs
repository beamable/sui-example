using UnityEngine;

namespace MoeBeam.Game.Scripts.Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Animator primaryAnimator;

        #endregion

        #region PRIVATE_VARIABLES

        private static readonly int ThrustHash = Animator.StringToHash("thrust");
        private static readonly int SwingHash = Animator.StringToHash("swing");

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        #endregion

        #region PUBLIC_METHODS
        
        public void PlayThrustAnimation()
        {
            primaryAnimator.SetTrigger(ThrustHash);
        }

        public void PlaySwingAnimation()
        {
            primaryAnimator.SetTrigger(SwingHash);
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion


        
    }
}