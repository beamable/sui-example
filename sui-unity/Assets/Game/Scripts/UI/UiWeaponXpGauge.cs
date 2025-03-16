using System;
using DG.Tweening;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class UiWeaponXpGauge : MonoBehaviour
    {
        [Header("Player Health")]
        [SerializeField] private Image xpBar;
        [SerializeField] private Image xpBarTrail;
        [SerializeField] private float drainSpeed = 0.25f;
        [SerializeField] private float trailDelay = 0.4f;
        [SerializeField] private bool forRanged = false;

        private void Start()
        {
            xpBar.fillAmount = 0f;
            xpBarTrail.fillAmount = 0f;
            
            EventCenter.Subscribe(GameData.OnWeaponGainedXpEvent, OnWeaponGainedXp);
        }

        private void OnWeaponGainedXp(object obj)
        {
            if(obj is not WeaponInstance weapon) return;
            switch (forRanged)
            {
                case true when weapon.AttackType != GameData.AttackType.Shoot:
                case false when weapon.AttackType == GameData.AttackType.Shoot:
                    return;
            }

            var currentXp = weapon.MetaData.Xp;
            var maxXp = XpManager.Instance.XpGainData.defaultXpThreshold;
            var ratio = (float) currentXp / maxXp;
            
            var sequence = DOTween.Sequence();
            sequence.Append(xpBar.DOFillAmount(ratio, drainSpeed));
            sequence.AppendInterval(trailDelay);
            sequence.Append(xpBarTrail.DOFillAmount(ratio, drainSpeed));
            
            sequence.Play();
        }
    }
    
}