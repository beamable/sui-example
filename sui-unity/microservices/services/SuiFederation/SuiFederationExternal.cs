using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.SuiFederation.Endpoints;
using SuiFederationCommon;

namespace Beamable.SuiFederation;

public partial class SuiFederation : IFederatedInventory<SuiWeb3ExternalIdentity>
{
    async Promise<FederatedAuthenticationResponse> IFederatedLogin<SuiWeb3ExternalIdentity>.Authenticate(string token, string challenge, string solution)
    {
        return await Provider.GetService<AuthenticateExternalEndpoint>()
            .Authenticate(token, challenge, solution);
    }

    Promise<FederatedInventoryProxyState> IFederatedInventory<SuiWeb3ExternalIdentity>.GetInventoryState(string id)
    {
        return new Promise<FederatedInventoryProxyState>();
    }

    Promise<FederatedInventoryProxyState> IFederatedInventory<SuiWeb3ExternalIdentity>.StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<FederatedItemCreateRequest> newItems, List<FederatedItemDeleteRequest> deleteItems,
        List<FederatedItemUpdateRequest> updateItems)
    {
        return new Promise<FederatedInventoryProxyState>();
    }
}