using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using SuiFederationCommon.FederationContent;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MoeBeam.Game.Scripts.Beam
{
    public class BeamWeaponContentManager : GenericSingleton<BeamWeaponContentManager>
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] List<WeaponItemRef> weaponsRefs;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES
        
        public List<WeaponInstance> WeaponContents { get; private set; }

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            await UniTask.WaitUntil(() => BeamManager.IsReady);
            await ResolveWeaponContents();
        }

        #endregion

        #region PUBLIC_METHODS
        
        public int GetOwnedWeaponsCount()
        {
            return WeaponContents.FindAll(w => w.IsOwned).Count;
        }
        
        public WeaponInstance GetItemByInstanceId(long instanceId)
        {
            return WeaponContents.Find(w => w.InstanceId == instanceId);
        }

        public WeaponInstance GetOwnedMeleeWeapon()
        {
            try
            {
                return WeaponContents.Find(w => (w.AttackType != GameData.AttackType.Shoot && w.IsOwned));
            }
            catch (Exception e)
            {
                return null;
            } 
        }
        
        public WeaponInstance GetOwnedRangedWeapon()
        {
            try
            {
                return WeaponContents.Find(w => (w.AttackType == GameData.AttackType.Shoot && w.IsOwned));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        #region PRIVATE_METHODS

        private async UniTask ResolveWeaponContents()
        {
            WeaponContents = new List<WeaponInstance>();
            foreach (var weapon in weaponsRefs)
            {
                var resolvedW = await weapon.Resolve();
                var icon = await GetSpriteAsync(resolvedW.icon);
                Sprite bulletIcon = null;
                
                resolvedW.CustomProperties.TryGetValue(GameData.DamageKey, out var damageValue);
                int.TryParse(damageValue, out var damage);
                resolvedW.CustomProperties.TryGetValue(GameData.AttackSpeedKey, out var attackSpeedValue);
                float.TryParse(attackSpeedValue, out var attackSpeed);
                resolvedW.CustomProperties.TryGetValue(GameData.AttackTypeKey, out var attackTypeValue);
                int.TryParse(attackTypeValue, out var attackType);
                var type = GameData.ToAttackType(attackType);
                
                var metaData = new WeaponMetaData(0, 1, damage, attackSpeed);
                var weaponInstance = new WeaponInstance(icon, bulletIcon, 0, resolvedW.Id, resolvedW.name, 
                   resolvedW.Description, type, metaData);
                WeaponContents.Add(weaponInstance);
            }
        }
        
        private async UniTask<Sprite> GetSpriteAsync(AssetReferenceSprite spriteReference)
        {
            if (spriteReference == null)
            {
                Debug.LogError("Sprite reference is null.");
                return null;
            }

            if (!spriteReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"Invalid sprite reference: {spriteReference}");
                return null;
            }
            try
            {
                // Ensure the asset is not already loaded
                if (!spriteReference.OperationHandle.IsValid() || spriteReference.OperationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    var spriteHandle = spriteReference.LoadAssetAsync();
                    await spriteHandle;

                    if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        return spriteHandle.Result;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load sprite: {spriteHandle.OperationException}");
                        return null;
                    }
                }
                else
                {
                    return spriteReference.OperationHandle.Result as Sprite;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while loading sprite: {e.Message}");
                return null;
            }
        }

        #endregion

        
    }
}