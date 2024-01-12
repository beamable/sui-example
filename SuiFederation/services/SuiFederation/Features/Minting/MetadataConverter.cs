using System.Collections.Generic;
using Beamable.SuiFederation.Features.Minting.Models;
using SuiFederationCommon.Content;
using Beamable.Common.Api.Inventory;
using SuiFederationCommon.Models;

namespace Beamable.SuiFederation.Features.Minting;

internal static class MetadataConverter
{
    public static GameItem ToGameItem(this MintRequest request, BlockchainItem? contentDefinition)
    {
        return new GameItem
        {
            Name = contentDefinition?.Name ?? request.ContentId,
            Description = contentDefinition?.Description ?? "",
            ImageURL = contentDefinition?.Image ?? ""
        };
    }

    public static IEnumerable<ItemProperty> GetProperties(this SuiObject suiObject)
    {
        var properties = new List<ItemProperty>();

        if (!string.IsNullOrEmpty(suiObject.name))
            properties.Add(new ItemProperty { name = "Name", value = suiObject.name });

        if (!string.IsNullOrEmpty(suiObject.description))
            properties.Add(new ItemProperty { name = "Description", value = suiObject.description });

        if (!string.IsNullOrEmpty(suiObject.image_url))
            properties.Add(new ItemProperty { name = "ImageURL", value = suiObject.image_url });

        return properties;
    }
}