using UnityEngine;

namespace SuiFederationCommon.Models
{
    public class SuiBalance
    {
        [SerializeField]
        public string? coinType;
        [SerializeField]
        public long total;
    }
}