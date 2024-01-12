using System.Collections.Generic;

namespace Beamable.SuiFederation.Features.Minting;

public class MintRequest
{
    public string ContentId { get; set; }
    public uint Amount { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}