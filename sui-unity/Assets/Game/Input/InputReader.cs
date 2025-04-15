using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MoeBeam.Game.Input
{
    public interface IInputReader
    {
        void EnablePlayerInputActions();
        void DisablePlayerInputActions();
    }
    
    [CreateAssetMenu(fileName = "InputReader", menuName = "Moe/Data/InputReader")]
    public class InputReader : ScriptableObject, BeamInputActions.IPlayerActions, IInputReader
    {
        #region Actions

        public event UnityAction<Vector2> MoveEvent = delegate { };
        public event UnityAction PrimaryAttackEvent = delegate { }; 
        public event UnityAction SecondaryAttackEvent = delegate { }; 
        public event UnityAction ForceRestartEvent = delegate { }; 
        public event UnityAction EscapePressedEvent = delegate { }; 

        #endregion

        #region Variables

        private BeamInputActions _inputActions;

        public Vector2 MousePos { get; private set; } = Vector2.zero;

        #endregion

        #region Status

        public void EnablePlayerInputActions()
        {
            if(_inputActions == null)
            {
                _inputActions = new BeamInputActions();
                _inputActions.Enable();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Player.Enable();
        }

        public void DisablePlayerInputActions()
        {
            _inputActions.Player.Disable();
        }

        #endregion

        #region Callbacks

        
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            MousePos = context.ReadValue<Vector2>();
        }

        public void OnPrimaryAttack(InputAction.CallbackContext context)
        {
            if(context.performed)
                PrimaryAttackEvent?.Invoke();
        }

        public void OnSecondaryAttack(InputAction.CallbackContext context)
        {
            if(context.performed)
                SecondaryAttackEvent?.Invoke();
        }

        public void OnForceRestart(InputAction.CallbackContext context)
        {
            if(context.performed)
                ForceRestartEvent?.Invoke();

        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if(context.performed)
                EscapePressedEvent?.Invoke();
        }

        #endregion
        
    }
}
