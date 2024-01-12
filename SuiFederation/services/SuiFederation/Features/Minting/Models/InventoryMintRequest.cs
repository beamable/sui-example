using System.Collections.Generic;

namespace Beamable.SuiFederation.Features.Minting.Models;

public class InventoryMintRequest
{
    public CurrencyItem? CurrencyItem { get; set; }
    public List<GameItem> GameItems { get; set; }
}