using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Beam;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Helpers;
using MoeBeam.Game.Scripts.Items;
using UnityEngine;

namespace MoeBeam.Game.Scripts.UI
{
    public class UiMainMenuWeaponChooser : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private WeaponCard weaponCardPrefab;
        [SerializeField] private Transform meleeTransform;
        [SerializeField] private Transform rangedTransform;
        [SerializeField] private BeamButton selectButton;

        #endregion

        #region PRIVATE_VARIABLES

        private WeaponCard _selectedMeleeCard;
        private WeaponCard _selectedRangedCard;
        private List<WeaponCard> _meleeCards = new List<WeaponCard>();
        private List<WeaponCard> _rangedCards = new List<WeaponCard>();

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            selectButton.AddListener(OnSelectWeapons);
            selectButton.gameObject.SetActive(false);
            foreach (var weapon in BeamWeaponContentManager.Instance.WeaponContents)
            {
                var weaponCard = Instantiate(weaponCardPrefab, weapon.AttackType == GameData.AttackType.Shoot ? rangedTransform : meleeTransform);
                weaponCard.SetWeaponCard(weapon, this);
                weaponCard.transform.name = weapon.DisplayName + "_Card";
                weaponCard.gameObject.SetActive(true);
                if(weapon.AttackType == GameData.AttackType.Shoot) 
                    _rangedCards.Add(weaponCard);
                else 
                    _meleeCards.Add(weaponCard);
            }
        }

        private void Update()
        {
            if (_selectedMeleeCard == null || _selectedRangedCard == null || selectButton.gameObject.activeInHierarchy) return;
            selectButton.gameObject.SetActive(true);
        }

        #endregion

        #region PUBLIC_METHODS

        public void SetSelectedMelee(WeaponCard card)
        {
            _selectedMeleeCard = card;
            DeselectMeleeCards();
        }
        
        public void SetSelectedRanged(WeaponCard card)
        {
            _selectedRangedCard = card;
            DeselectRangedCards();
        }

        #endregion

        #region PRIVATE_METHODS
        
        private void DeselectMeleeCards()
        {
            foreach (var card in _meleeCards)
            {
                card.SelectCard(false);
            }
        }

        private void DeselectRangedCards()
        {
            foreach (var card in _rangedCards)
            {
                card.SelectCard(false);
            }
        }

        private async void OnSelectWeapons()
        {
            //TODO debug
            try
            {
                selectButton.SwitchText(false, "Adding weapons to your inventory...");
                await BeamInventoryManager.Instance.AddItemToInventory(_selectedMeleeCard.CurrentWeapon);
                await BeamInventoryManager.Instance.AddItemToInventory(_selectedRangedCard.CurrentWeapon);
                UiMainMenuManager.Instance.SetFinalId().Forget();
                selectButton.ButtonCurrent.interactable = false;
            }
            catch (Exception e)
            {
                selectButton.SwitchText(true);
                selectButton.ButtonCurrent.interactable = true;
                Debug.LogError($"On Select weapons failed: {e.Message}");
            }
        }


        #endregion


    }
}