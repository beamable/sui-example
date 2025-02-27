using System.Threading.Tasks;
using Jering.Javascript.NodeJS;

namespace SuiFederationCommon.Node
{
    /// <summary>
    /// Service for calling SUI SDK functions
    /// </summary>
    public static class NodeService
    {
        private const string BridgeModulePath = "js/bridge.js";

        /// <summary>
        /// Creates SUI wallet keypair
        /// </summary>
        /// <returns></returns>
        public static async Task<string> CreateWallet()
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "createWallet");
        }

        /// <summary>
        /// Import SUI wallet from a private key
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ImportWallet(string privateKey)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "importWallet",
                new object[] { privateKey });
        }

        /// <summary>
        /// Import SUI wallet from a private key
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> VerifySignature(string token, string challenge, string solution)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<bool>(
                BridgeModulePath,
                "verifySignature",
                new object[] { token, challenge, solution });
        }

        /// <summary>
        /// Mint regular coin
        /// </summary>
        /// <param name="mintRequestJson"></param>
        /// <param name="realmAccountPrivateKey"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<string> MintRegularCoin(string mintRequestJson, string realmAccountPrivateKey, string environment)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "mintRegularCoin",
                new object[] { mintRequestJson, realmAccountPrivateKey, environment });
        }

        /// <summary>
        /// Burn regular coin
        /// </summary>
        /// <param name="mintRequestJson"></param>
        /// <param name="realmAccountPrivateKey"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<string> BurnRegularCoin(string mintRequestJson, string realmAccountPrivateKey, string environment)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "burnCoins",
                new object[] { mintRequestJson, realmAccountPrivateKey, environment });
        }

        /// <summary>
        /// CoinBalance
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="requestJson"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<string> CoinBalance(string wallet, string requestJson, string environment)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "getBalance",
                new object[] { wallet, requestJson, environment });
        }

        /// <summary>
        /// Mint NFTs
        /// </summary>
        /// <param name="mintRequestJson"></param>
        /// <param name="realmAccountPrivateKey"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<string> MintNfts(string mintRequestJson, string realmAccountPrivateKey, string environment)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "mintNfts",
                new object[] { mintRequestJson, realmAccountPrivateKey, environment });
        }

        /// <summary>
        /// GetOwnedObjects
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="packageId"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<string> GetOwnedObjects(string wallet, string packageId, string environment)
        {
            return await StaticNodeJSService.InvokeFromFileAsync<string>(
                BridgeModulePath,
                "getOwnedObjects",
                new object[] { wallet, packageId, environment });
        }
    }
}