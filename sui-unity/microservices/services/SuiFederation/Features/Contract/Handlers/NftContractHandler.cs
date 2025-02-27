using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.SuiFederation.Features.Common;
using Beamable.SuiFederation.Features.Contract.Exceptions;
using Beamable.SuiFederation.Features.Contract.Handlers.Models;
using Beamable.SuiFederation.Features.Contract.Storage.Models;
using Beamable.SuiFederation.Features.Contract.SuiClientWrapper;
using Beamable.SuiFederation.Features.Contract.SuiClientWrapper.Models;
using HandlebarsDotNet;
using SuiFederationCommon.Extensions;

namespace Beamable.SuiFederation.Features.Contract.Handlers;

public class NftContractHandler(
    ContractService contractService,
    SuiClient suiClient) : IService, IContentContractHandler
{
    public async Task HandleContract(IContentObject clientContentInfo)
    {
        try
        {
            var itemContent = clientContentInfo as ItemContent;
            var moduleName = FederationContentExtensions.SanitizeModuleName(itemContent!.ToModuleName());
            if (await contractService.ContractExists<NftContract>(itemContent!.ContentType))
                return;

            var itemTemplate = await File.ReadAllTextAsync("Features/Contract/Templates/nft.move");
            var template = Handlebars.Compile(itemTemplate);
            var itemResult = template(new NftTemplate(moduleName));
            var contractPath = $"{SuiFederationConfig.ContractSourcePath}{moduleName}.move";
            await ContractWriter.WriteContract(contractPath, itemResult);

            var deployOutput = await suiClient.CompileAndPublish(moduleName);

            await contractService.InsertContract(new NftContract
            {
                PackageId = deployOutput.GetPackageId(),
                Module = moduleName,
                ContentId = itemContent!.ContentType,
                AdminCap = GetAdminCap(deployOutput, moduleName),
            });

            BeamableLogger.Log($"Created contract for {moduleName}");
        }
        catch (Exception e)
        {
            throw new ContractException($"Error in creating contract for {clientContentInfo.GetType()}, exception: {e.Message}");
        }
    }

    private string GetAdminCap(MoveDeployOutput deployOutput, string moduleName)
        => deployOutput.CreatedObjects.FirstOrDefault(obj => obj.ObjectType.StartsWith($"{deployOutput.GetPackageId()}::{moduleName}::AdminCap"))?.ObjectId
           ?? throw new ContractException("AdminCap not found.");
}