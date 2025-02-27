using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.SuiFederation.Features.Common;
using Beamable.SuiFederation.Features.Contract.Exceptions;
using Beamable.SuiFederation.Features.Contract.Storage.Models;
using Beamable.SuiFederation.Features.Contract.SuiClientWrapper;
using Beamable.SuiFederation.Features.Contract.SuiClientWrapper.Models;
using HandlebarsDotNet;
using SuiFederationCommon.Extensions;
using SuiFederationCommon.FederationContent;

namespace Beamable.SuiFederation.Features.Contract.Handlers;

public class CoinCurrencyHandler(
    ContractService contractService,
    SuiClient suiClient) : IService, IContentContractHandler
{
    public async Task HandleContract(IContentObject clientContentInfo)
    {
        try
        {
            if (await contractService.ContractExists<CoinContract>(clientContentInfo.Id))
                return;

            if (clientContentInfo is not CoinCurrency coinCurrency)
                throw new ContractException($"{clientContentInfo.Id} is not a {nameof(CoinCurrency)}");

            var itemTemplate = await File.ReadAllTextAsync("Features/Contract/Templates/coin.move");
            var template = Handlebars.Compile(itemTemplate);
            var itemResult = template(coinCurrency);
            var contractPath = $"{SuiFederationConfig.ContractSourcePath}{coinCurrency.ToModuleName()}.move";
            await ContractWriter.WriteContract(contractPath, itemResult);

            var deployOutput = await suiClient.CompileAndPublish(coinCurrency.ToModuleName());

            await contractService.InsertContract(new CoinContract
            {
                PackageId = deployOutput.GetPackageId(),
                Module = coinCurrency.ToModuleName(),
                ContentId = coinCurrency.Id,
                TreasuryCap = GetCurrencyTreasuryCap(deployOutput),
                AdminCap = GetAdminCap(deployOutput, coinCurrency.ToModuleName())
            });

            BeamableLogger.Log($"Created contract for {coinCurrency.Id}");
        }
        catch (Exception e)
        {
            throw new ContractException($"Error in creating contract for {clientContentInfo.Id}, exception: {e.Message}");
        }
    }

    private string GetCurrencyTreasuryCap(MoveDeployOutput deployOutput)
        => deployOutput.CreatedObjects.FirstOrDefault(obj => obj.ObjectType.StartsWith($"0x2::coin::TreasuryCap<{deployOutput.GetPackageId()}"))?.ObjectId
           ?? throw new ContractException("TreasuryCap not found.");

    private string GetAdminCap(MoveDeployOutput deployOutput, string moduleName)
        => deployOutput.CreatedObjects.FirstOrDefault(obj => obj.ObjectType.StartsWith($"{deployOutput.GetPackageId()}::{moduleName}::AdminCap"))?.ObjectId
           ?? throw new ContractException("AdminCap not found.");
}