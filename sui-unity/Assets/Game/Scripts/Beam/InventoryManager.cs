﻿using System;
using System.Linq;
using Beamable;
using Beamable.Common.Api.Inventory;
using Beamable.Server.Clients;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Beam
{
    public class InventoryManager : GenericSingleton<InventoryManager>
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private string weaponContentId = "items.weapon";

        #endregion

        #region PRIVATE_VARIABLES

        private BeamContext _beamContext = BeamManager.BeamContext;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            await UniTask.WaitUntil(() => AccountManager.Instance.NewAccountCreated);
            _beamContext = BeamManager.BeamContext;
            _beamContext.Api.InventoryService.Subscribe(OnRefresh);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public async UniTask AddItemToInventory(WeaponInstance weapon)
        {
            var add =  BeamManager.BeamContext.Api.InventoryService.AddItem(weapon.ContentId,
                weapon.MetaData.ToDictionary(true)).IsCompleted;
            Debug.LogWarning($"ADDING ITEM TO INVENTORY {add}");
            //await BeamManager.SkullClient.GrantItem(weapon.ContentId, weapon.MetaData.ToDictionary());
        }
        
        #endregion

        #region PRIVATE_METHODS
        
        private async void OnRefresh(InventoryView obj)
        {
            var inventoryItems = await _beamContext.Inventory.LoadItems();
            var weapons = inventoryItems.Where(item => item.ContentId.Contains(weaponContentId)).ToArray();
            Debug.LogWarning($"REFRESHING INVENTORY {weapons.Length}");
            if(weapons.Length < 1) return;
            foreach (var item in weapons)
            {
                foreach (var weapon in WeaponContentManager.Instance.WeaponContents.Where(weapon => weapon.ContentId == item.ContentId))
                {
                    weapon.InstanceId = item.ItemId;
                }
            }
        }

        #endregion


    }
}