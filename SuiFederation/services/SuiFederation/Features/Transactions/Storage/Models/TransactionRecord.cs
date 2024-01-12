using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.SuiFederation.Features.Transactions.Storage.Models;

public class TransactionRecord
{
    [BsonElement("_id")]
    public string Id { get; set; } = null!;
    public long UserId { get; set; }
    public string WalletAddress { get; set; } = null!;
    public DateTime ExpireAt { get; set; } = DateTime.Now.AddDays(1);
    public TransactionState State { get; set; }
}
public enum TransactionState
{
    Inserted = 0,
    Confirmed = 1,
    Failed = 100
}