using UnityEngine;

namespace SuiFederationCommon.Models
{
    public class SuiBalance
    {
        [SerializeField]
        public SuiCoinBalance[]? coins;
    }

    public class SuiCoinBalance
    {
        [SerializeField]
        public string? coinType;
        [SerializeField]
        public long total;
    }
}