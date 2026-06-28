using Pharmaceutical.Core.DTOs;

namespace Pharmaceutical.Core.Interfaces;

public interface IDrugService
{
    Task<PagedResult<DrugDto>> GetPagedAsync(string? search, int page, int pageSize);
    Task<DrugDto?> GetByIdAsync(string drugId);
    Task<List<DrugDto>> GetLowStockAsync(int threshold);
    Task<bool> AddAsync(DrugCreateDto dto);
    Task<bool> UpdateAsync(string drugId, DrugUpdateDto dto);
    Task<bool> DeleteAsync(string drugId);
}
