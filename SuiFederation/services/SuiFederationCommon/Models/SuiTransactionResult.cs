using UnityEngine;

namespace SuiFederationCommon.Models
{
    public class SuiTransactionResult
    {
        [SerializeField]
        public string status;
        [SerializeField]
        public string digest;
        [SerializeField]
        public string computationCost;
        [SerializeField]
        public string[] objectIds;
        [SerializeField]
        public string error;
    }
}