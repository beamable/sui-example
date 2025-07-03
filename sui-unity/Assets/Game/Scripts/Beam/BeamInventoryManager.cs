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
            await UniTask.WaitUntil(() => BeamAccountManager.Instance.IsReady);
            _beamContext = BeamManager.BeamContext;
            _inventoryUpdateBuilder = new InventoryUpdateBuilder();
            SetupCoins();
            _beamContext.Api.InventoryService.Subscribe(OnRefresh);
        }

        #endregion

        #region PUBLIC_METHODS
        
        public async UniTask AddItemToInventory(WeaponInstance weapon)
        {
            try
            {
                var add = BeamManager.BeamContext.Api.InventoryService.AddItem(weapon.ContentId,
                    weapon.MetaData.ToDictionary(true)).IsCompleted;
                Debug.LogWarning($"ADDING ITEM TO INVENTORY {add}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to add ITEM to INVENTORY: {e}");
            }
        }

        public async UniTask UpdateCurrency(GameData.CoinType coinType, bool deduct = false)
        {
            //var inventoryBuilder = new InventoryUpdateBuilder();
            foreach (var coin in PlayerCoins.Where(coin => coin.CoinType == coinType))
            {
                if(deduct && coin.Amount == 0) {return;}
                Debug.Log($"updating Currency for {coin.ContentId} with deduct = {deduct}");
                var amount = deduct ? -1 : +1;
                coin.Amount += amount;
                _inventoryUpdateBuilder.CurrencyChange(coin.ContentId, amount);
                Dictionary<PlayerCoin, bool> updated = new Dictionary<PlayerCoin, bool> {{coin, deduct}};
                EventCenter.InvokeEvent(GameData.OnCoinCollectedEvent, updated);
                break;
            }
            await _beamContext.Inventory.Update(_inventoryUpdateBuilder);
        }
        
        public PlayerCoin GetCoinByType(GameData.CoinType coinType)
        {
            return PlayerCoins.FirstOrDefault(coin => coin.CoinType == coinType);
        }
        
        #endregion

        #region PRIVATE_METHODS
        
        [ContextMenu("Refresh")]
        private void OnRefreshing()
        {
            RefreshInventory().Forget();
        }
        private void OnRefresh(InventoryView obj)
        {
            RefreshInventory().Forget();
        }

        private async UniTask RefreshInventory()
        {
            var inventoryItems = await _beamContext.Inventory.LoadItems();
            var weapons = inventoryItems.Where(item => item.ContentId.Contains(weaponContentId)).ToArray();
            RefreshWeapons(weapons);
        }

        private static void RefreshWeapons(PlayerItem[] weapons)
        {
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