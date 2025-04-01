using System;
using Beamable.Server.Clients;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Managers
{
    public class XpManager : GenericSingleton<XpManager>
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private XpGainData xpGainData;
        [SerializeField] private AudioClip levelUpSfx;
        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES
        
        public XpGainData XpGainData => xpGainData;

        #endregion

        #region UNITY_CALLS

        private void OnEnable()
        {
            EventCenter.Subscribe(GameData.OnEnemyKillRewardEvent, OnEnemyKilled);
        }

        private void OnDisable()
        {
            EventCenter.Unsubscribe(GameData.OnEnemyKillRewardEvent, OnEnemyKilled);
        }

        #endregion

        #region PUBLIC_METHODS
        
        private void OnEnemyKilled(object obj)
        {
            if (obj is not EnemyKilledData data) return;
            CheckForLevelUp(data.Xp, data.InstanceId);
            
        }
        
        private void CheckForLevelUp(int xpReward, long instanceId)
        {
            var nextThreshold = xpGainData.defaultXpThreshold;
            var currentXp = 0;
            var weapon = BeamWeaponContentManager.Instance.GetItemByInstanceId(instanceId);
            if(weapon.MetaData.Level >= xpGainData.maxLevel) return;
            // if(weapon.MetaData.Level > 1)
            //     nextThreshold = (int) (xpGainData.defaultXpThreshold * (1 + (weapon.MetaData.Level / xpGainData.xpDivider)));
            currentXp = weapon.MetaData.Xp + xpReward;
            weapon.MetaData.Update(currentXp, weapon.MetaData.Level, weapon.MetaData.CurrentDamage, weapon.MetaData.CurrentAttackSpeed);
            EventCenter.InvokeEvent(GameData.OnWeaponGainedXpEvent, weapon);
            if (currentXp >= nextThreshold)
            {
                LevelUp(weapon).Forget();
            }
        }

        private async UniTask LevelUp(WeaponInstance weapon)
        {
            var nextLevel = 1;
            var newDamage = 0;
            var newSpeed = 0f;
            var newXp = 0;
            if(weapon.MetaData.Level < xpGainData.maxLevel)
            {
                nextLevel = weapon.MetaData.Level + 1;
                if(weapon.MetaData.CurrentDamage < xpGainData.maxDamage)
                {
                    newDamage = weapon.MetaData.CurrentDamage + xpGainData.damageIncrease;
                }
                
                if(weapon.MetaData.CurrentAttackSpeed > xpGainData.minAttackSpeed)
                {
                    newSpeed = weapon.MetaData.CurrentAttackSpeed - xpGainData.attackSpeedDecrease;
                }
                AudioManager.Instance.PlaySfx(levelUpSfx);
            }
            else
            {
                newXp = xpGainData.defaultXpThreshold;
            }
            weapon.MetaData.Update(newXp, nextLevel, newDamage, newSpeed);
            EventCenter.InvokeEvent(weapon.AttackType != GameData.AttackType.Shoot ? 
                    GameData.OnMeleeLeveledUpEvent : GameData.OnRangedLeveledUpEvent, weapon);
            EventCenter.InvokeEvent(GameData.OnWeaponGainedXpEvent, weapon);
            
            await BeamManager.BeamContext.Api.InventoryService.UpdateItem(weapon.ContentId, weapon.InstanceId, weapon.MetaData.ToDictionary());
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion


    }
}