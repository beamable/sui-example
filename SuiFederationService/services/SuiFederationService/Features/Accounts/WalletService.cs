using System.Threading.Tasks;
using Beamable.Common;
using Beamable.SuiFederationService;
using HDWallet.Core;
using Nethereum.Signer;
using Rijndael256;
using SuiFederationService.Features.Accounts;
using SuiFederationService.Features.Accounts.Storage;
using SuiFederationService.Features.Accounts.Storage.Models;

namespace SuiFederationService.Features.Wallets;

public class WalletService : IService
{
    private const string RealmWalletName = "game-wallet";

    private readonly VaultCollection _vaultCollection;
    private readonly Configuration _configuration;
    
    private SuiWallet? _cachedRealmWallet;

    public WalletService(VaultCollection vaultCollection, Configuration configuration)
    {
        _vaultCollection = vaultCollection;
        _configuration = configuration;
    }
    
    public async Task<SuiWallet> GetOrCreateWallet(string walletName)
    {
        var wallet = await GetWallet(walletName);
        if (wallet is null)
        {
            wallet = await CreateWallet(walletName);
            if (wallet is null)
            {
                BeamableLogger.LogWarning("Wallet already created, fetching again");
                return await GetOrCreateWallet(walletName);
            }

            BeamableLogger.Log("Saved wallet {walletName} -> {walletAddress}", walletName, wallet.Address);
        }

        return wallet;
    }
    
    public async ValueTask<SuiWallet> GetOrCreateRealmWallet()
    {
        if (_cachedRealmWallet is not null)
            return _cachedRealmWallet;

        var wallet = await GetWallet(RealmWalletName);
        if (wallet is null)
        {
            wallet = await CreateWallet(RealmWalletName);
            if (wallet is null)
            {
                BeamableLogger.LogWarning("Wallet already created, fetching again");
                return await GetOrCreateWallet(RealmWalletName);
            }

            _cachedRealmWallet = wallet;
            BeamableLogger.Log("Saved wallet {walletName} -> {walletAddress}", RealmWalletName, wallet.Address);
            BeamableLogger.LogWarning("Please add some gas money to your wallet {walletAddress} to be able to pay for fees.", wallet.Address);
        }
        
        return wallet;
    }
    
    private async Task<SuiWallet?> GetWallet(string walletName)
    {
        var vault = await _vaultCollection.GetVaultByName(walletName);
        if (vault is null)
        {
            return null;
        }

        var privateKey = Rijndael.Decrypt(vault.PrivateKeyEncrypted, _configuration.RealmSecret, KeySize.Aes256);
        return new SuiWallet(privateKey);
    }
    
    private async Task<SuiWallet?> CreateWallet(string walletName)
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
        var newWallet = new SuiWallet(privateKeyBytes);

        return await _vaultCollection.TryInsertVault(new Vault
        {
            Name = walletName,
            AddressHex = newWallet.Address,
            PrivateKeyEncrypted = Rijndael.Encrypt(newWallet.PrivateKeyBytes.ToHexString(), _configuration.RealmSecret, KeySize.Aes256)
        }) ? newWallet : null;
    }
    
}