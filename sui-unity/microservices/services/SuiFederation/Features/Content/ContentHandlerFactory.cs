using System;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.SuiFederation.Features.Content.FunctionMessages;
using Beamable.SuiFederation.Features.Content.Handlers;
using Microsoft.Extensions.DependencyInjection;
using SuiFederationCommon.FederationContent;
using SuiFederationCommon.Models;

namespace Beamable.SuiFederation.Features.Content;

public class ContentHandlerFactory(
    IDependencyProvider serviceProvider) : IService
{
    // public IContentHandler GetHandler(string contentId)
    // {
    //     return contentId switch
    //     {
    //         not null when contentId.StartsWith($"{FederationContentExtensions.RegularCoinPrefix}") => serviceProvider.GetRequiredService<RegularCoinHandler>(),
    //         _ => throw new NotSupportedException($"ContentId '{contentId}' is not supported.")
    //     };
    // }

    public IContentHandler GetHandler(IContentObject contentObject)
    {
        return contentObject switch
        {
            CoinCurrency => serviceProvider.GetRequiredService<RegularCoinHandler>(),
            INftBase => serviceProvider.GetRequiredService<NftHandler>(),
            _ => throw new NotSupportedException($"ContentId '{contentObject.Id}' is not supported.")
        };
    }

    public IContentHandler GetHandler(Type messageType)
    {
        return messageType switch
        {
            not null when messageType == typeof(RegularCoinMintMessage) => serviceProvider.GetRequiredService<RegularCoinHandler>(),
            not null when messageType == typeof(RegularCoinBurnMessage) => serviceProvider.GetRequiredService<RegularCoinHandler>(),
            not null when messageType == typeof(NftMintMessage) => serviceProvider.GetRequiredService<NftHandler>(),
            _ => throw new NotSupportedException($"Message '{messageType}' is not supported.")
        };
    }

}