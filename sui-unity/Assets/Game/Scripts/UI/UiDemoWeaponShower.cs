using DG.Tweening;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Items;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class UiDemoWeaponShower : MonoBehaviour
    {
        [SerializeField] private CanvasGroup meleeGroup;
        [SerializeField] private CanvasGroup rangedGroup;
        [SerializeField] private WeaponCard meleeWeaponCard;
        [SerializeField] private WeaponCard rangedWeaponCard;

        public void ShowWeaponCard()
        {
            meleeWeaponCard.SetWeaponCard(WeaponContentManager.Instance.GetOwnedMeleeWeapon(), null, false);
            rangedWeaponCard.SetWeaponCard(WeaponContentManager.Instance.GetOwnedRangedWeapon(), null, false);
            
            var sequence = DOTween.Sequence();
            sequence.Append(meleeGroup.DOFade(1, 2f));
            sequence.Join(rangedGroup.DOFade(1, 2f));
            sequence.Join(meleeWeaponCard.transform.DOScale(Vector3.one * 1.75f, 2f));
            sequence.Join(rangedWeaponCard.transform.DOScale(Vector3.one * 1.75f, 2f));
            sequence.Play();
        }
    }
}