using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pharmaceutical.Core.Interfaces;
using Pharmaceutical.Core.DTOs;
using Moq;
using Xunit;

namespace Pharmaceutical.Tests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var drugServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDrugService));
                if (drugServiceDescriptor != null)
                    services.Remove(drugServiceDescriptor);

                var mockDrugService = new Mock<IDrugService>();
                mockDrugService.Setup(s => s.GetPagedAsync(null, 1, 20))
                    .ReturnsAsync(new PagedResult<DrugDto>());

                services.AddSingleton(mockDrugService.Object);
            });
        });
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy_OrServiceUnavailableWithoutDb()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK ||
            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable);
    }
}
