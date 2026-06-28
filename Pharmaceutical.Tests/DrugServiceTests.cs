using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pharmaceutical.Core;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;
using Pharmaceutical.Services;

namespace Pharmaceutical.Tests;

public class DrugServiceTests
{
    private readonly Mock<IDrugRepository> _repositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly DrugService _service;

    public DrugServiceTests()
    {
        _service = new DrugService(
            _repositoryMock.Object,
            _cacheMock.Object,
            NullLogger<DrugService>.Instance);
    }

    [Fact]
    public async Task AddAsync_ClearsCache_OnSuccess()
    {
        var dto = new DrugCreateDto { DrugId = "D001", DrugName = "测试药品" };
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<DrugCatalogEntity>())).ReturnsAsync(true);

        var result = await _service.AddAsync(dto);

        Assert.True(result);
        _cacheMock.Verify(c => c.RemoveAsync("AllDrugs", default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ClearsCache_OnSuccess()
    {
        _repositoryMock.Setup(r => r.DeleteAsync("D001")).ReturnsAsync(true);

        var result = await _service.DeleteAsync("D001");

        Assert.True(result);
        _cacheMock.Verify(c => c.RemoveAsync("AllDrugs", default), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ReturnsFalse_WhenNameEmpty()
    {
        var dto = new DrugCreateDto { DrugId = "D001", DrugName = "" };
        var result = await _service.AddAsync(dto);
        Assert.False(result);
    }

    [Fact]
    public async Task GetPagedAsync_MapsResults()
    {
        _repositoryMock.Setup(r => r.GetPagedAsync(null, 1, 20))
            .ReturnsAsync(new PagedResult<DrugCatalogEntity>
            {
                Items = new List<DrugCatalogEntity>
                {
                    new() { DrugId = "D001", DrugName = "阿司匹林" }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 20
            });

        var result = await _service.GetPagedAsync(null, 1, 20);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("阿司匹林", result.Items[0].DrugName);
    }
}
