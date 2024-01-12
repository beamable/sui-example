using System.Collections.Generic;

namespace Beamable.SuiFederation.Features.Minting.Models;

public class GameItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageURL { get; set; }
    public IDictionary<string, string> CustomProperties { get; set; }
}