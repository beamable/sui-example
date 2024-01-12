using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.SuiFederation;
using Beamable.SuiFederation.Features.Minting;
using Beamable.SuiFederation.Features.SuiApi;
using Beamable.SuiFederation.Features.SuiApi.Models;
using Rijndael256;
using SuiFederation.Features.Accounts;
using SuiFederation.Features.Accounts.Storage;
using SuiFederation.Features.Accounts.Storage.Models;

namespace SuiFederation.Features.Wallets;

public class WalletService : IService
{
    private readonly SuiApiService _suiApiServiceService;
    private readonly Configuration _configuration;
    private readonly VaultCollection _vaultCollection;

    public WalletService(SuiApiService suiApiServiceService, Configuration configuration, VaultCollection vaultCollection)
    {
        _suiApiServiceService = suiApiServiceService;
        _configuration = configuration;
        _vaultCollection = vaultCollection;
    }

    public async Task<SuiCapObject> InitializeObjects()
    {
        if (string.IsNullOrWhiteSpace(await _configuration.PackageId) || string.IsNullOrWhiteSpace(await _configuration.SecretKey))
        {
            throw new ConfigurationException($"Couldn't initialize cap objects, publish the smart contract and store the packageId and secret key in the realm config.");
        }
        var caps = await _suiApiServiceService.InitializeObjects();
        _configuration.GameAdminCap = caps.GameAdminCap;
        _configuration.TreasuryCap = caps.TreasuryCap;
        return caps;
    }

    public async Task<SuiWallet> GetOrCreateWallet(string userId)
    {
        var wallet = await GetWallet(userId);
        if (wallet is null)
        {
            wallet = await CreateWallet(userId);
            BeamableLogger.Log("Saved wallet {walletName} -> {walletAddress}", userId, wallet?.Address);
        }
        return wallet;
    }

    private async Task<SuiWallet?> GetWallet(string userId)
    {
        var vault = await _vaultCollection.GetVaultByName(userId);
        if (vault is null)
        {
            return null;
        }

        var privateKey = Rijndael.Decrypt(vault.PrivateKeyEncrypted, _configuration.RealmSecret, KeySize.Aes256);
        return new SuiWallet
        {
            Address = vault.AddressHex,
            PrivateKey = privateKey
        };
    }

    private async Task<SuiWallet?> CreateWallet(string userId)
    {
        var suiKeys = await SuiApiService.ExportPrivateKey();
        var newWallet = new SuiWallet
        {
            Address = suiKeys.Public,
            PrivateKey = suiKeys.Private
        };

        return await _vaultCollection.TryInsertVault(new Vault
        {
            Name = userId,
            AddressHex = newWallet.Address,
            PrivateKeyEncrypted = Rijndael.Encrypt(newWallet.PrivateKey, _configuration.RealmSecret, KeySize.Aes256)
        }) ? newWallet : null;
    }

    public async Task<bool> VerifySignature(string token, string challenge, string solution)
    {
        return await _suiApiServiceService.VerifySignature(token, challenge, solution);
    }

    public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
    {
        var coinModule = await _configuration.CoinModule;
        var coinBalance = await _suiApiServiceService.GetBalance(id, coinModule);
        var suiObjects = await _suiApiServiceService.GetOwnedObjects(id);

        var items = new List<(string, FederatedItemProxy)>();
        var currencies = new Dictionary<string, long>
        {
            {
                coinModule, coinBalance.total
            }
        };

        foreach (var suiObject in suiObjects)
        {
            items.Add((suiObject.name,
                    new FederatedItemProxy
                    {
                        proxyId = suiObject.objectId,
                        properties = suiObject.GetProperties().ToList()
                    }
                ));
        }

        var itemGroups = items
            .GroupBy(i => i.Item1)
            .ToDictionary(g => g.Key, g => g.Select(i => i.Item2).ToList());

        return new FederatedInventoryProxyState
        {
            currencies = currencies,
            items = itemGroups
        };
    }
}