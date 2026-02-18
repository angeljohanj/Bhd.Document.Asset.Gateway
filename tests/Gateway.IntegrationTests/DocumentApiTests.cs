using FluentAssertions;
using Gateway.Application.DTOs;
using Gateway.Domain.Entities;
using Gateway.Domain.Enums;
using Gateway.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace Gateway.IntegrationTests;

public class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

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
    }

    [Fact]
    public async Task UploadDocument_ReturnsAccepted()
    {
        var client = _factory.CreateClient();
        var request = new DocumentUploadRequestDto
        {
            Filename = "integration.pdf",
            EncodedFile = "JVBERi0xLjQKJ corpse==",
            ContentType = "application/pdf",
            DocumentType = DocumentType.CONTRACT,
            Channel = Channel.BRANCH
        };

        var response = await client.PostAsJsonAsync("/api/bhd/mgmt/1/documents/actions/upload", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<DocumentUploadResponseDto>();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchDocuments_ReturnsStoredDocuments()
    {
        var client = _factory.CreateClient();

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
        var documents = await response.Content.ReadFromJsonAsync<List<DocumentAsset>>();
        documents.Should().NotBeNull();
        documents.Should().Contain(d => d.Filename == "search_me.txt");
    }
}
