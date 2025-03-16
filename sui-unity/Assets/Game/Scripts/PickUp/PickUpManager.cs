using System;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;

namespace Game.Scripts.PickUp
{
    //Keeping it basic with one already placed pickup, turn on and off
    public class PickUpManager : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private HealthPickUp healthPickUp;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            healthPickUp.gameObject.SetActive(false);
            EventCenter.Subscribe(GameData.OnTenEnemiesKilledEvent, TurnOnPickUp);
            EventCenter.Subscribe(GameData.OnBossActivateEvent, TurnOnPickUp);
        }

        #endregion

        #region PUBLIC_METHODS
        
        private void TurnOnPickUp(object obj)
        {
            healthPickUp.gameObject.SetActive(true);
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}