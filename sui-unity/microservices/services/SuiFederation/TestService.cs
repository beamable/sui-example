using System.Threading.Tasks;
using Beamable.SuiFederation.Features.Contract;

namespace Beamable.SuiFederation;

public class TestService : IService
{
    private readonly ContractService _contractService;

    public TestService(ContractService contractService)
    {
        _contractService = contractService;
    }

    public async Task Test()
    {
        // await foreach (var contentObject in _contractService.FetchFederationContent())
        // {
        //     var i = 0;
        // }
        await _contractService.InitializeContentContracts();
    }
}