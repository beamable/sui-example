using System.Collections.Generic;

namespace Beamable.SuiFederation.Features.Minting.Models;

public class InventoryMintRequest
{
    public List<CurrencyItem> CurrencyItems { get; set; } = new();
    public List<GameItem> GameItems { get; set; } = new();
}