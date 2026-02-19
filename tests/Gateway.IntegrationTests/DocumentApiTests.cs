using FluentAssertions;
using Gateway.Api;
using Gateway.Application.DTOs;
using Gateway.Domain.Entities;
using Gateway.Domain.Enums;
using Gateway.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gateway.IntegrationTests;

public class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private HttpClient CreateClientWithAuth()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "BHD_Secret_Key_2026");
        return client;
    }

    [Fact]
    public async Task UploadDocument_ReturnsAccepted()
    {
        var client = CreateClientWithAuth();
        var request = new DocumentUploadRequestDto
        {
            Filename = "integration.pdf",
            EncodedFile = "JVBERi0xLjQKJWRmY3QK", 
            ContentType = "application/pdf",
            DocumentType = DocumentType.CONTRACT,
            Channel = Channel.BRANCH
        };

        var response = await client.PostAsJsonAsync("/api/bhd/mgmt/1/documents/actions/upload", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<DocumentUploadResponseDto>(_jsonOptions);
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchDocuments_ReturnsStoredDocuments()
    {
        var client = CreateClientWithAuth();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Documents.Add(new DocumentAsset
            {
                Filename = "search_me.txt",
                ContentType = "text/plain",
                DocumentType = DocumentType.FORM,
                Channel = Channel.BACKOFFICE,
                Status = DocumentStatus.SENT
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/bhd/mgmt/1/documents?filename=search_me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResponseDto<DocumentAsset>>(_jsonOptions);
        responseData.Should().NotBeNull();
        responseData!.Items.Should().Contain(d => d.Filename == "search_me.txt");
        responseData.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task RequestWithoutKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync("/api/bhd/mgmt/1/documents");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
