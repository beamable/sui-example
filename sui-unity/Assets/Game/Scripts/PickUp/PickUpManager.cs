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
        
        [SerializeField] private Transform pickUpLocation;
        [SerializeField] private HealthPickUp healthPickUpPrefab;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            EventCenter.Subscribe(GameData.OnTenEnemiesKilledEvent, TurnOnPickUp);
            EventCenter.Subscribe(GameData.OnBossActivateEvent, TurnOnPickUp);
        }

        #endregion

        #region PUBLIC_METHODS
        
        private void TurnOnPickUp(object obj)
        {
            Instantiate(healthPickUpPrefab, pickUpLocation.position, Quaternion.identity);
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        
    }
}