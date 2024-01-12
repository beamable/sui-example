using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SuiFederation.Features.Accounts.Storage.Models;

public record Vault
{
    [BsonElement("_id")]
    public string Name { get; set; } = default!;

    public DateTime Created { get; set; } = DateTime.Now;
    public string AddressHex { get; set; } = default!;
    public string PrivateKeyEncrypted { get; set; } = default!;
}