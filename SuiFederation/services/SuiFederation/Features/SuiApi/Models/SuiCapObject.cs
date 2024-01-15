using System.Collections.Generic;

namespace Beamable.SuiFederation.Features.SuiApi.Models;

public class SuiCapObject
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class SuiCapObjects
{
    public List<SuiCapObject> GameAdminCaps { get; set; } = new();
    public List<SuiCapObject> TreasuryCaps { get; set; } = new();
}