using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;

namespace Pharmaceutical.Infrastructure.Repositories;

public class DrugRepository : IDrugRepository
{
    private readonly PharmaceuticalDbContext _context;

    public DrugRepository(PharmaceuticalDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DrugCatalogEntity>> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _context.Drugs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.DrugName.Contains(search) ||
                d.DrugId.Contains(search) ||
                (d.TradeName != null && d.TradeName.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.DrugId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<DrugCatalogEntity>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DrugCatalogEntity?> GetByIdAsync(string drugId) =>
        await _context.Drugs.FirstOrDefaultAsync(d => d.DrugId == drugId);

    public async Task<List<DrugCatalogEntity>> GetLowStockAsync(int threshold) =>
        await _context.Drugs.Where(d => d.StockQuantity < threshold).OrderBy(d => d.StockQuantity).ToListAsync();

    public async Task<bool> AddAsync(DrugCatalogEntity drug)
    {
        var exists = await _context.Drugs.AnyAsync(d => d.DrugId == drug.DrugId);
        if (exists) return false;

        await _context.Drugs.AddAsync(drug);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(DrugCatalogEntity drug)
    {
        _context.Drugs.Update(drug);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string drugId)
    {
        var drug = await GetByIdAsync(drugId);
        if (drug == null) return false;

        // Check for related stock transactions before deleting
        var hasTransactions = await _context.StockTransactions
            .AnyAsync(t => t.DrugId == drugId);
        if (hasTransactions)
            throw new InvalidOperationException("该药品存在库存流水记录，无法删除。请先下架或联系管理员。");

        _context.Drugs.Remove(drug);
        return await _context.SaveChangesAsync() > 0;
    }
}
