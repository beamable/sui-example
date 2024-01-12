using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Beamable.Server;
using Beamable.SuiFederation.Features.Transactions.Storage.Models;
using MongoDB.Driver;

namespace Beamable.SuiFederation.Features.Transactions.Storage;

public class TransactionCollection : IService
{
    private readonly IStorageObjectConnectionProvider _storageObjectConnectionProvider;
    private IMongoCollection<TransactionRecord>? _collection;

    public TransactionCollection(IStorageObjectConnectionProvider storageObjectConnectionProvider)
    {
        _storageObjectConnectionProvider = storageObjectConnectionProvider;
    }

    private async ValueTask<IMongoCollection<TransactionRecord>> Get()
    {
        if (_collection is null)
        {
            var db = await _storageObjectConnectionProvider.SuiFederationStorageDatabase();
            _collection = db.GetCollection<TransactionRecord>("transaction");

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<TransactionRecord>(
                    Builders<TransactionRecord>.IndexKeys
                        .Ascending(x => x.ExpireAt),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }
                )
            );
        }
        return _collection;
    }

    public async Task<TransactionRecord> GetTransaction(Expression<Func<TransactionRecord, bool>> filter)
    {
        var collection = await Get();
        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> TryInsertTransaction(TransactionRecord transactionRecord)
    {
        var collection = await Get();
        try
        {
            await collection.InsertOneAsync(transactionRecord);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task SaveState(string transactionId, TransactionState transactionState)
    {
        var collection = await Get();
        await collection.UpdateOneAsync(x => x.Id == transactionId,
            Builders<TransactionRecord>.Update
                .Set(x => x.State, transactionState)
        );
    }

    public async Task DeleteTransaction(string transactionId)
    {
        var collection = await Get();
        await collection.DeleteOneAsync(x => x.Id == transactionId);
    }
}