using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.SuiFederation.Features.Minting.Models;
using Beamable.SuiFederation.Features.SuiApi;
using SuiFederationCommon.Content;
using Beamable.Server.Content;
using Beamable.SuiFederation.Features.Minting.Exceptions;
using Beamable.SuiFederation.Features.Transactions;
using Beamable.SuiFederation.Features.Transactions.Storage.Models;
using SuiFederation.Features.Wallets;

namespace Beamable.SuiFederation.Features.Minting;

public class MintingService : IService
{
    private readonly ContentService _contentService;
    private readonly SuiApiService _suiApiServiceService;
    private readonly TransactionManager _transactionManager;
    private readonly WalletService _walletService;

    public MintingService(ContentService contentService, SuiApiService suiApiServiceService, TransactionManager transactionManager, WalletService walletService)
    {
        _contentService = contentService;
        _suiApiServiceService = suiApiServiceService;
        _transactionManager = transactionManager;
        _walletService = walletService;
    }

    public async Task Mint(long userId, string toWalletAddress, string inventoryTransactionId, ICollection<MintRequest> requests)
    {
        var existingTransaction = await _transactionManager.GetTransaction(inventoryTransactionId);
        if (existingTransaction is not null && existingTransaction.State == TransactionState.Confirmed )
        {
            return;
        }

        var mintRequest = new InventoryMintRequest();

        foreach (var request in requests)
        {
            var contentDefinition = await _contentService.GetContent(request.ContentId);

            switch (contentDefinition)
            {
                case BlockchainCurrency blockchainCurrency:
                    var currencyItem = request.ToCurrencyItem(blockchainCurrency);
                    var treasuryCap = _walletService.GetTreasuryCap(currencyItem.ModuleName);
                    if (treasuryCap is not null)
                    {
                        currencyItem.TreasuryCap = treasuryCap.Id;
                        mintRequest.CurrencyItems.Add(currencyItem);
                    }
                    break;
                case BlockchainItem blockchainItem:
                    var inventoryItem = request.ToGameItem(blockchainItem);
                    var gameCap = _walletService.GetGameCap(inventoryItem.ModuleName);
                    if (gameCap is not null)
                    {
                        inventoryItem.GameAdminCap = gameCap.Id;
                        mintRequest.GameItems.Add(inventoryItem);
                    }
                    break;
                default:
                    throw new UndefinedItemException(nameof(contentDefinition));
            }
        }

        var result = await _suiApiServiceService.MintInventoryItems(toWalletAddress, mintRequest);
        if (result.error is null)
        {
            await _transactionManager.MarkConfirmed(inventoryTransactionId);
        }
        else
        {
            await _transactionManager.MarkFailed(inventoryTransactionId);
        }

    }
}