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
using SuiFederationCommon.Extensions;
using SuiFederationCommon.Models;

namespace Beamable.SuiFederation.Features.Content.Handlers;

public class NftHandler(
    ContractService contractService,
    SuiApiService suiApiService,
    TransactionManagerFactory transactionManagerFactory,
    MintCollection mintCollection) : IService, IContentHandler
{
    public async Task<BaseMessage?> ConstructMessage(string transaction, string wallet, InventoryRequest inventoryRequest, IContentObject contentObject)
    {
        var contract = await contractService.GetByContentId<NftContract>(inventoryRequest.ToNftType());
        var nftItem = NftContentItemExtensions.Create(inventoryRequest, (contentObject as INftBase)!);
        return new NftMintMessage(
            inventoryRequest.ContentId,
            contract.PackageId,
            contract.Module,
            "mint",
            wallet,
            contract.AdminCap,
            nftItem);
    }

    public async Task SendMessages(string transaction, List<BaseMessage> messages)
    {
        var transactionManager = transactionManagerFactory.Create(transaction);
        var mintMessages = messages.OfType<NftMintMessage>().ToList();
        try
        {
            var result = await suiApiService.MintNfts(mintMessages);
            await transactionManager.AddChainTransaction(new ChainTransaction
            {
                Digest = result.digest,
                Error = result.error,
                Function = $"{nameof(NftHandler)}.{nameof(SendMessages)}",
                GasUsed = result.gasUsed,
                Data = mintMessages.SerializeSelected(),
                Status = result.status,
            });
            if (result.status != "success")
            {
                var message = $"{nameof(NftHandler)}.{nameof(SendMessages)} failed with status {result.status}";
                BeamableLogger.LogError(message);
                await transactionManager.TransactionError(transaction, new Exception(message));
            }
            else
            {
                await mintCollection.InsertMints(
                    mintMessages.Select(m => new Mint
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
                $"{nameof(NftHandler)}.{nameof(SendMessages)} failed with error {e.Message}";
            BeamableLogger.LogError(message);
            await transactionManager.TransactionError(transaction, new Exception(message));
        }
    }

    public async Task<IFederatedState> GetState(string wallet, string contentId)
    {
        var contentTypePart = contentId.ToContentType();
        var contract = await contractService.GetByContentId<NftContract>(contentTypePart);
        var ownedObjects = await suiApiService.GetOwnedObjects(wallet, new GetOwnedObjectsRequest(contract.PackageId));
        return new ItemsState
        {
            Items = ownedObjects.ToInventoryState()
        };
    }
}