using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.SuiFederation.Features.Minting.Models;
using Beamable.SuiFederation.Features.SuiApi;
using SuiFederationCommon.Content;
using Beamable.Server.Content;
using Beamable.SuiFederation.Features.Transactions;
using Beamable.SuiFederation.Features.Transactions.Storage.Models;

namespace Beamable.SuiFederation.Features.Minting;

public class MintingService : IService
{
    private readonly ContentService _contentService;
    private readonly SuiApiService _suiApiServiceService;
    private readonly Configuration _configuration;
    private readonly TransactionManager _transactionManager;

    public MintingService(ContentService contentService, SuiApiService suiApiServiceService, Configuration configuration, TransactionManager transactionManager)
    {
        _contentService = contentService;
        _suiApiServiceService = suiApiServiceService;
        _configuration = configuration;
        _transactionManager = transactionManager;
    }

    public async Task Mint(long userId, string toWalletAddress, string inventoryTransactionId, ICollection<MintRequest> requests)
    {
        var existingTransaction = await _transactionManager.GetTransaction(inventoryTransactionId);
        if (existingTransaction is not null && existingTransaction.State == TransactionState.Confirmed )
        {
            return;
        }

        var mintRequest = new InventoryMintRequest
        {
            GameItems = new List<GameItem>()
        };

        foreach (var request in requests)
        {
            var contentDefinition = await _contentService.GetContent(request.ContentId);

            switch (contentDefinition)
            {
                case BlockchainCurrency blockchainCurrency:
                    if (blockchainCurrency.CoinModule == await _configuration.CoinModule)
                    {
                        mintRequest.CurrencyItem = new CurrencyItem
                        {
                            Amount = request.Amount
                        };
                    }
                    break;
                case BlockchainItem blockchainItem:
                    mintRequest.GameItems.Add(request.ToGameItem(blockchainItem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(contentDefinition));
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