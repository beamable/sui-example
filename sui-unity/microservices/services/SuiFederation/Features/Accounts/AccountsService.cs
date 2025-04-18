using System;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Api.Autogenerated.Accounts;
using Beamable.Common;
using Beamable.SuiFederation.Caching;
using Beamable.SuiFederation.Features.Accounts.Models;
using Beamable.SuiFederation.Features.Accounts.Storage;
using Beamable.SuiFederation.Features.Accounts.Storage.Models;
using Beamable.SuiFederation.Features.SuiApi;

namespace Beamable.SuiFederation.Features.Accounts;

public class AccountsService : IService
{
    private const string RealmAccountName = "default-account";
    private readonly VaultCollection _vaultCollection;
    private Account? _cachedRealmAccount;
    private readonly Configuration _configuration;
    private readonly AccountsApi _accountsApi;
    private readonly MemoryCache<Account> _accountsCache = new(TimeSpan.FromMinutes(30));

    public AccountsService(VaultCollection vaultCollection, Configuration configuration, AccountsApi accountsApi)
    {
        _vaultCollection = vaultCollection;
        _configuration = configuration;
        _accountsApi = accountsApi;
    }

    public async Task<Account> GetOrCreateAccount(string accountName)
    {
        var account = await GetAccount(accountName);
        if (account is null)
        {
            account = await CreateAccount(accountName);
            if (account is null)
            {
                BeamableLogger.LogWarning("Account already created, fetching again");
                return await GetOrCreateAccount(accountName);
            }

            BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", accountName, account.Address);
        }
        return account;
    }

    public async ValueTask<Account> GetOrCreateRealmAccount()
    {
        if (_cachedRealmAccount is not null)
            return _cachedRealmAccount;

        var account = await GetAccount(RealmAccountName);
        if (account is null)
        {
            account = await CreateAccount(RealmAccountName);
            if (account is null)
            {
                BeamableLogger.LogWarning("Account already created, fetching again");
                return await GetOrCreateRealmAccount();
            }

            BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", RealmAccountName,
                account.Address);
            BeamableLogger.LogWarning(
                "Please add some gas money to your account {accountAddress} to be able to pay for fees.",
                account.Address);
        }
        _cachedRealmAccount = account;
        return account;
    }

    public async ValueTask<Account> GetOrImportRealmAccount(string privateKey)
    {
        if (_cachedRealmAccount is not null)
            return _cachedRealmAccount;

        var account = await GetAccount(RealmAccountName);
        if (account is null)
        {
            account = await CreateAccount(RealmAccountName, privateKey);
            if (account is null)
            {
                BeamableLogger.LogWarning("Account already created, fetching again");
                return await GetOrImportRealmAccount(privateKey);
            }

            BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", RealmAccountName,
                account.Address);
            BeamableLogger.LogWarning(
                "Please add some gas money to your account {accountAddress} to be able to pay for fees.",
                account.Address);
        }
        _cachedRealmAccount = account;
        return account;
    }

    public async ValueTask<Account?> GetRealmAccount()
    {
        if (_cachedRealmAccount is not null)
            return _cachedRealmAccount;

        var account = await GetAccount(RealmAccountName);
        if (account is not null)
        {
            _cachedRealmAccount = account;
            return account;
        }
        BeamableLogger.LogWarning("Realm account {accountName} does not exist.", RealmAccountName);
        return null;
    }

    public async Task<Account> ImportAccount(string id, string privateKey)
    {
        var existingAccount = await GetAccount(id);
        if (existingAccount is not null)
            return existingAccount;

        var account = await CreateAccount(id, privateKey);
        if (account is not null) return account;
        BeamableLogger.LogWarning("Account already created, fetching again");
        return await ImportAccount(id, privateKey);
    }

    public async Task<Account?> GetAccount(string accountName)
    {
        return await _accountsCache.GetOrAddAsync(accountName, async () =>
        {
            var vault = await _vaultCollection.GetVaultByName(accountName);
            if (vault is null)
            {
                return null;
            }
            var privateKey = EncryptionService.Decrypt(vault.PrivateKey, _configuration.RealmSecret);
            return new Account(vault.Name, vault.Address, privateKey);
        }) ?? null;
    }

    private async Task<Account?> CreateAccount(string accountName)
    {
        var keys = await SuiApiService.CreateWallet();
        var privateKeyEncrypted = EncryptionService.Encrypt(keys.PrivateKey, _configuration.RealmSecret);
        var newAccount = new Account(accountName, keys.Address, keys.PrivateKey);

        return await _vaultCollection.TryInsertVault(new Vault(accountName, newAccount.Address, privateKeyEncrypted, keys.PublicKey))
            ? newAccount
            : null;
    }

    private async Task<Account?> CreateAccount(string accountName, string privateKey)
    {
        var keys = await SuiApiService.ImportPrivateKey(privateKey);
        var privateKeyEncrypted = EncryptionService.Encrypt(keys.PrivateKey, _configuration.RealmSecret);
        var newAccount = new Account(accountName, keys.Address, keys.PrivateKey);

        return await _vaultCollection.TryInsertVault(new Vault(accountName, newAccount.Address, privateKeyEncrypted, keys.PublicKey))
            ? newAccount
            : null;
    }

    private async Task<string?> GetGamerTagByWalletAddress(string address)
    {
        return await _vaultCollection.GetNameByAddress(address);
    }

    public async Task<Account?> GetAccountByAddress(string address)
    {
        var name = await GetGamerTagByWalletAddress(address);
        if (!string.IsNullOrWhiteSpace(name))
            return await GetAccount(name);

        BeamableLogger.LogError($"Account for {address} not found");
        return null;
    }

    public static async Task<bool> IsSignatureValid(string token, string challenge, string solution)
    {
        return await SuiApiService.VerifySignature(token, challenge, solution);
    }

    public async Task<Beamable.Api.Autogenerated.Models.Account?> SearchAccount(string query)
    {
        var searchResponse = await _accountsApi.GetSearch(1, 1, query);
        return searchResponse?.accounts.FirstOrDefault();
    }
}