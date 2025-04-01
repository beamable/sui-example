using System;
using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Docs;
using Beamable.Common.Inventory;
using Beamable.Player;
using Beamable.Server.Clients;
using Cysharp.Threading.Tasks;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Beam
{
    [Serializable]
    public class PlayerCoin
    {
        public string ContentId;
        public int Amount;
        public GameData.CoinType CoinType;
        
        public PlayerCoin(string contentId, int amount, GameData.CoinType coinType)
        {
            ContentId = contentId;
            Amount = amount;
            CoinType = coinType;
        }
    }
    
    public class BeamInventoryManager : GenericSingleton<BeamInventoryManager>
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private string weaponContentId = "items.weapon";
        [SerializeField] private float coinUpdateInterval = 1f;
        [SerializeField] private List<CurrencyRef> currencyRefs = new List<CurrencyRef>();

        #endregion

        #region PRIVATE_VARIABLES

        private float _nextCoinUpdate = 0f;
        private BeamContext _beamContext = BeamManager.BeamContext;
        private InventoryUpdateBuilder _inventoryUpdateBuilder;

        #endregion

        #region PUBLIC_VARIABLES
        
        public List<PlayerCoin> PlayerCoins { get; private set; }

        #endregion

        #region UNITY_CALLS

        private async void Start()
        {
            await UniTask.WaitUntil(() => BeamAccountManager.Instance.NewAccountCreated);
            _beamContext = BeamManager.BeamContext;
            _inventoryUpdateBuilder = new InventoryUpdateBuilder();
            SetupCoins();
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

        public async UniTask UpdateCurrency(CoinData coinData)
        {
            foreach (var coin in PlayerCoins.Where(coin => coin.CoinType == coinData.coinType))
            {
                coin.Amount += coinData.GetCoinValue();
                _inventoryUpdateBuilder.CurrencyChange(coin.ContentId, coin.Amount);
                EventCenter.InvokeEvent(GameData.OnCoinCollectedEvent, coin);
                break;
            }

            if (_nextCoinUpdate < Time.time)
            {
                _nextCoinUpdate = Time.time + coinUpdateInterval;
                await _beamContext.Inventory.Update(_inventoryUpdateBuilder);
            }
        }
        
        #endregion

        #region PRIVATE_METHODS
        
        private void OnRefresh(InventoryView obj)
        {
            RefreshInventory(obj).Forget();
        }

        private async UniTask RefreshInventory(InventoryView obj)
        {
            var inventoryItems = await _beamContext.Inventory.LoadItems();
            var weapons = inventoryItems.Where(item => item.ContentId.Contains(weaponContentId)).ToArray();
            RefreshWeapons(weapons);
        }

        private static void RefreshWeapons(PlayerItem[] weapons)
        {
            Debug.LogWarning($"REFRESHING INVENTORY {weapons.Length}");
            if(weapons.Length < 1) return;
            foreach (var item in weapons)
            {
                foreach (var weapon in BeamWeaponContentManager.Instance.WeaponContents.Where(weapon => weapon.ContentId == item.ContentId))
                {
                    weapon.InstanceId = item.ItemId;
                }
            }
        }
        
        private void SetupCoins()
        {
            PlayerCoins = new List<PlayerCoin>();
            foreach (var currencyRef in currencyRefs)
            {
                var coin = new PlayerCoin(currencyRef.Id, 0, ParseCoinType(currencyRef.Id));
                PlayerCoins.Add(coin);
            }
        }

        private GameData.CoinType ParseCoinType(string contentId)
        {
            if (contentId.Contains("gold")) return GameData.CoinType.Gold;
            if (contentId.Contains("star")) return GameData.CoinType.Star;
            
            return GameData.CoinType.Beam;
        }
        

        #endregion


    }
}