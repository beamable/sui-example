using Beamable.Common.Content;
using Beamable.Common.Inventory;
using UnityEngine;

namespace SuiFederationCommon.Content
{
    /// <summary>
    /// BlockchainCurrency
    /// </summary>
    [ContentType("blockchain_currency")]
    public class BlockchainCurrency : CurrencyContent
    {
        [SerializeField] private string _coinModule;

        /// <summary>
        /// CoinModule
        /// </summary>
        public string CoinModule => _coinModule;
    }
}