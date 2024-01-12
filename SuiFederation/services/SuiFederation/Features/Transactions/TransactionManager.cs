using System;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.SuiFederation.Features.Transactions.Exceptions;
using Beamable.SuiFederation.Features.Transactions.Storage;
using Beamable.SuiFederation.Features.Transactions.Storage.Models;

namespace Beamable.SuiFederation.Features.Transactions;

public class TransactionManager : IService
{
    private readonly TransactionCollection _transactionCollection;

    public TransactionManager(TransactionCollection transactionCollection)
    {
        _transactionCollection = transactionCollection;
    }

    public async Task<T> WithTransactionAsync<T>(string id, string transaction, long userId, Func<Task<T>> handler)
    {
        await SaveTransaction(transaction, userId, id);
        try
        {
            return await handler();
        }
        catch (Exception ex)
        {
            BeamableLogger.LogError("Error processing transaction {transaction}. Clearing the transaction record to enable retries.", transaction);
            BeamableLogger.LogError(ex);
            await ClearTransaction(transaction);
            throw new TransactionException(ex.Message);
        }
    }

    public async Task<TransactionRecord> GetTransaction(string inventoryTransactionId)
    {
        return await _transactionCollection.GetTransaction((tr) => tr.Id == inventoryTransactionId);
    }

    private async Task SaveTransaction(string inventoryTransactionId, long userId, string walletAddress)
    {
        var isSuccess = await _transactionCollection.TryInsertTransaction(new TransactionRecord
        {
            Id = inventoryTransactionId,
            State = TransactionState.Inserted,
            UserId = userId,
            WalletAddress = walletAddress
        });
        if (!isSuccess)
        {
            throw new TransactionException($"Transaction {inventoryTransactionId} already processed or in-progress");
        }
    }

    public async Task MarkConfirmed(string inventoryTransactionId)
    {
        await _transactionCollection.SaveState(inventoryTransactionId, TransactionState.Confirmed);
    }

    public async Task MarkFailed(string inventoryTransactionId)
    {
        await _transactionCollection.SaveState(inventoryTransactionId, TransactionState.Failed);
    }

    private async Task ClearTransaction(string inventoryTransactionId)
    {
        await _transactionCollection.DeleteTransaction(inventoryTransactionId);
    }
}