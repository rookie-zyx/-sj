using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;
using Pharmaceutical.Core.Interfaces;

namespace Pharmaceutical.Infrastructure.Repositories;

public class StockRepository : IStockRepository
{
    private readonly PharmaceuticalDbContext _context;

    public StockRepository(PharmaceuticalDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockTransactionEntity>> GetAllAsync() =>
        await _context.StockTransactions
            .Include(t => t.Drug)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<bool> ProcessTransactionAsync(string drugId, string transactionType, int quantity, string operatorName, string? remark)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Row-level lock to prevent concurrent stock modification
        var drug = await _context.Drugs
            .FromSqlRaw("SELECT * FROM drugs WHERE drug_id = {0} FOR UPDATE", drugId)
            .FirstOrDefaultAsync();
        if (drug == null) return false;

        if (transactionType == "IN")
        {
            drug.StockQuantity += quantity;
        }
        else if (transactionType == "OUT")
        {
            if (drug.StockQuantity < quantity) return false;
            drug.StockQuantity -= quantity;
        }
        else
        {
            return false;
        }

        var stockTransaction = new StockTransactionEntity
        {
            DrugId = drugId,
            TransactionType = transactionType,
            Quantity = quantity,
            Operator = operatorName,
            CreatedAt = DateTime.UtcNow,
            Remark = remark
        };

        await _context.StockTransactions.AddAsync(stockTransaction);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
