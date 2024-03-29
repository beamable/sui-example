using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Newtonsoft.Json;
using UnityEngine;

namespace SuiFederationCommon.Content
{
    /// <summary>
    /// BlockchainItem
    /// </summary>
    [ContentType("blockchain_item")]
    public class BlockchainItem : ItemContent
    {
        [SerializeField] private string _name;
        [SerializeField][TextArea(10, 10)] private string _description;
        [SerializeField] private string _image;
        [SerializeField] private SerializableDictionaryStringToString _customProperties;

        /// <summary>
        /// NFT name
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// NFT description
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// NFT image
        /// </summary>
        public string Image => _image;

        /// <summary>
        /// NFT custom properties
        /// </summary>
        public IDictionary<string, string> CustomProperties => _customProperties;

        /// <summary>
        /// Creates a JSON string that represents the NFT metadata
        /// </summary>
        /// <returns></returns>
        public string ToMetadataJsonString()
        {
            var metadata = new
            {
                Name,
                Description,
                Image,
                CustomProperties
            };
            return JsonConvert.SerializeObject(metadata);
        }
    }
}