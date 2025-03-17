using System;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Input;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Player
{
    public class Player : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [Header("Move")]
        [SerializeField] private float moveSpeed = 10f;
        
        [Header("Aim")]
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float lookOffset = 0f;
        
        
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private PlayerHealth playerHealth;        
        #endregion

        #region PRIVATE_VARIABLES

        private Vector2 _moveDirection = Vector2.zero;
        
        //rotation
        private float _rotationAngle = 0f;
        private Vector2 _lookDirection = Vector2.zero;
        private Vector3 _worldMousePos = Vector3.zero;
        private Quaternion _targetRotation = Quaternion.identity;

        private Camera _mainCamera;
        private PlayerAnimationController _playerAnimationController;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS
        
        private void OnEnable()
        {
            inputReader.EnablePlayerInputActions();

            inputReader.MoveEvent += OnMove;
            playerAttack.InitModules(inputReader);
        }

        private void OnDisable()
        {
            inputReader.DisablePlayerInputActions();
            
            inputReader.MoveEvent -= OnMove;
            playerAttack.DeInitModules();
        }
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            _playerAnimationController = GetComponent<PlayerAnimationController>();
        }

        private void Start()
        {
            playerAttack.Init();
            playerHealth.Init(_playerAnimationController);
        }

        private void LateUpdate()
        {
            if(GameManager.Instance.GameEnded) return;
            RotateTowardsMouse();
        }

        private void FixedUpdate()
        {
            rb2D.linearVelocity = _moveDirection * moveSpeed;
        }

        #endregion

        #region PUBLIC_METHODS
        
        

        #endregion

        #region PRIVATE_METHODS
        
        private void OnMove(Vector2 direction)
        {
            if(!GameManager.Instance.HasGameStarted || GameManager.Instance.GameEnded) return;
            _moveDirection = direction;
        }

        
        private void RotateTowardsMouse()
        {
            _worldMousePos = _mainCamera.ScreenToWorldPoint(new Vector3(
                inputReader.MousePos.x, 
                inputReader.MousePos.y, 
                Mathf.Abs(_mainCamera.transform.position.z - transform.position.z))
            );

            _lookDirection = (Vector2)_worldMousePos - (Vector2)transform.position;
            _rotationAngle = Mathf.Atan2(_lookDirection.y, _lookDirection.x) * Mathf.Rad2Deg;

            _rotationAngle += lookOffset;

            _targetRotation = Quaternion.Euler(0f, 0f, _rotationAngle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                _targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
        
        
        #endregion


    }
}