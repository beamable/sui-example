using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.SuiFederation.Features.Accounts;
using Beamable.SuiFederation.Features.Content.FunctionMessages;
using Beamable.SuiFederation.Features.Content.Models;
using Beamable.SuiFederation.Features.Contract;
using Beamable.SuiFederation.Features.Contract.Storage.Models;
using Beamable.SuiFederation.Features.Inventory.Models;
using Beamable.SuiFederation.Features.Inventory.Storage;
using Beamable.SuiFederation.Features.Inventory.Storage.Models;
using Beamable.SuiFederation.Features.SuiApi;
using Beamable.SuiFederation.Features.SuiApi.Models;
using Beamable.SuiFederation.Features.Transactions;
using Beamable.SuiFederation.Features.Transactions.Storage.Models;

namespace Beamable.SuiFederation.Features.Content.Handlers;

public class GameCoinHandler(
    ContractService contractService,
    SuiApiService suiApiService,
    AccountsService accountsService,
    TransactionManagerFactory transactionManagerFactory,
    MintCollection mintCollection) : IService, IContentHandler
{
    public async Task<BaseMessage?> ConstructMessage(string transaction, string wallet, InventoryRequest inventoryRequest,
        IContentObject contentObject) =>
        inventoryRequest.Amount switch
    {
        > 0 => await PositiveAmountMessage(transaction, wallet, "mint", inventoryRequest),
        < 0 => await NegativeAmountMessage(transaction, wallet, "burn", inventoryRequest),
        _ => null
    };

    public Task<BaseMessage?> ConstructMessage(string transaction, string wallet, InventoryRequestUpdate inventoryRequest,
        IContentObject contentObject)
    {
        return Task.FromResult<BaseMessage?>(EmptyMessageExtensions.Create());
    }

    private async Task<GameCoinMintMessage> PositiveAmountMessage(string transaction, string wallet, string function, InventoryRequest inventoryRequest)
    {
        var contract = await contractService.GetByContentId<GameCoinContract>(inventoryRequest.ContentId);
        return new GameCoinMintMessage(
            inventoryRequest.ContentId,
            contract.PackageId,
            contract.Module,
            function,
            wallet,
            contract.AdminCap,
            contract.TokenPolicyCap,
            contract.TokenPolicy,
            contract.Store,
            inventoryRequest.Amount);
    }

    private async Task<GameCoinBurnMessage?> NegativeAmountMessage(string transaction, string wallet, string function, InventoryRequest inventoryRequest)
    {
        var transactionManager = transactionManagerFactory.Create(transaction);
        var contract = await contractService.GetByContentId<GameCoinContract>(inventoryRequest.ContentId);
        var playerAccount = await accountsService.GetAccountByAddress(wallet);
        var balance = await suiApiService.GetGameCoinBalance(wallet, new GameCoinBalanceRequest(contract.PackageId, contract.Module));
        if (balance.Total >= Math.Abs(inventoryRequest.Amount))
            return new GameCoinBurnMessage(
                inventoryRequest.ContentId,
                contract.PackageId,
                contract.Module,
                function,
                wallet,
                contract.AdminCap,
                contract.TokenPolicyCap,
                contract.TokenPolicy,
                contract.Store,
                Math.Abs(inventoryRequest.Amount),
                playerAccount!.PrivateKey);

        await transactionManager.AddChainTransaction(new ChainTransaction
        {
            Error = $"Insufficient funds for {inventoryRequest.ContentId}, balance is {balance.Total}, requested is {Math.Abs(inventoryRequest.Amount)}",
            Function = $"{nameof(GameCoinHandler)}.{nameof(NegativeAmountMessage)}",
            Status = "rejected",
        });
        return null;
    }

    public async Task SendMessages(string transaction, List<BaseMessage> messages)
    {
        if (messages.Count == 0) return;

        var mintMessages = messages.OfType<GameCoinMintMessage>().ToList();
        var burnMessages = messages.OfType<GameCoinBurnMessage>().ToList();

        if (mintMessages.Count > 0)
        {
            await SendPositiveAmountMessage(transaction, mintMessages);
        }

        if (burnMessages.Count > 0)
        {
            await SendNegativeMessage(transaction, burnMessages);
        }
    }

    private async Task SendPositiveAmountMessage(string transaction, List<GameCoinMintMessage> messages)
    {
        var transactionManager = transactionManagerFactory.Create(transaction);
        try
        {
            var result = await suiApiService.MintGameCurrency(messages);
            await transactionManager.AddChainTransaction(new ChainTransaction
            {
                Digest = result.digest,
                Error = result.error,
                Function = $"{nameof(GameCoinHandler)}.{nameof(SendPositiveAmountMessage)}",
                GasUsed = result.gasUsed,
                Data = messages.SerializeSelected(),
                Status = result.status,
            });
            if (result.status != "success")
            {
                var message = $"{nameof(GameCoinHandler)}.{nameof(SendPositiveAmountMessage)} failed with status {result.status}";
                BeamableLogger.LogError(message);
                await transactionManager.TransactionError(transaction, new Exception(message));
            }
            else
            {
                await mintCollection.InsertMints(
                    messages.Select(m => new Mint
                    {
                        PackageId = m.PackageId,
                        Module = m.Module,
                        Digest = result.digest,
                        ContentId = m.ContentId,
                        InitialOwnerAddress = m.PlayerWalletAddress,
                        Metadata = m.ToMetadata()
                    }));
            }
        }
        catch (Exception e)
        {
            var message =
                $"{nameof(GameCoinHandler)}.{nameof(SendPositiveAmountMessage)} failed with error {e.Message}";
            BeamableLogger.LogError(message);
            await transactionManager.TransactionError(transaction, new Exception(message));
        }
    }

    private async Task SendNegativeMessage(string transaction, List<GameCoinBurnMessage> messages)
    {
        var transactionManager = transactionManagerFactory.Create(transaction);
        try
        {
            var result = await suiApiService.BurnGameCurrency(messages);
            await transactionManager.AddChainTransaction(new ChainTransaction
            {
                Digest = result.digest,
                Error = result.error,
                Function = $"{nameof(GameCoinHandler)}.{nameof(SendNegativeMessage)}",
                GasUsed = result.gasUsed,
                Data = messages.SerializeSelected(),
                Status = result.status,
            });
            if (result.status != "success")
            {
                var message = $"{nameof(GameCoinHandler)}.{nameof(SendNegativeMessage)} failed with status {result.status}";
                BeamableLogger.LogError(message);
                await transactionManager.TransactionError(transaction, new Exception(message));
            }
        }
        catch (Exception e)
        {
            var message =
                $"{nameof(GameCoinHandler)}.{nameof(SendNegativeMessage)} failed with error {e.Message}";
            BeamableLogger.LogError(message);
            await transactionManager.TransactionError(transaction, new Exception(message));
        }
    }

    public async Task<IFederatedState> GetState(string wallet, string contentId)
    {
        var contract = await contractService.GetByContentId<GameCoinContract>(contentId);
        var balance = await suiApiService.GetGameCoinBalance(wallet, new GameCoinBalanceRequest(contract.PackageId, contract.Module));
        return new CurrenciesState
        {
            Currencies = new Dictionary<string, long>
            {
                { contentId, balance.Total }
            }
        };
    }
}