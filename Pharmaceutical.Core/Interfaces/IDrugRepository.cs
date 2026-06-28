using Pharmaceutical.Core.DTOs;

namespace Pharmaceutical.Core.Interfaces;

public interface IDrugRepository
{
    Task<PagedResult<DrugCatalogEntity>> GetPagedAsync(string? search, int page, int pageSize);
    Task<DrugCatalogEntity?> GetByIdAsync(string drugId);
    Task<List<DrugCatalogEntity>> GetLowStockAsync(int threshold);
    Task<bool> AddAsync(DrugCatalogEntity drug);
    Task<bool> UpdateAsync(DrugCatalogEntity drug);
    Task<bool> DeleteAsync(string drugId);
}
