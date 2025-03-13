using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MoeBeam.Game.Scripts.Beam
{
    public class WeaponContentManager : GenericSingleton<WeaponContentManager>
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] List<WeaponsRef> weaponsRefs;

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
        
        public WeaponInstance GetItemByInstanceId(long instanceId)
        {
            return WeaponContents.Find(w => w.InstanceId == instanceId);
        }

        public WeaponInstance GetOwnedMeleeWeapon()
        {
            return WeaponContents.Find(w => (w.AttackType != GameData.AttackType.Shoot && w.IsOwned));
        }
        
        public WeaponInstance GetOwnedRangedWeapon()
        {
            return WeaponContents.Find(w => (w.AttackType == GameData.AttackType.Shoot && w.IsOwned));
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
                var bulletIcon = await GetSpriteAsync(resolvedW.BulletIcon);
                var metaData = new WeaponMetaData(0, 1, resolvedW.Damage, resolvedW.AttackSpeed);
                var weaponInstance = new WeaponInstance(icon, bulletIcon, 0, resolvedW.Id, resolvedW.name, 
                    resolvedW.WeaponDescription, resolvedW.AttackType, metaData);
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