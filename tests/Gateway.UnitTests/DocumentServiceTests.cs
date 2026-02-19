using FluentAssertions;
using Gateway.Application.DTOs;
using Gateway.Application.Features;
using Gateway.Application.Interfaces;
using Gateway.Domain.Entities;
using Gateway.Domain.Enums;
using Moq;

namespace Gateway.UnitTests;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IDocumentPublisher> _publisherMock;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _publisherMock = new Mock<IDocumentPublisher>();
        _service = new DocumentService(_repositoryMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldPersistMetadataAndReturnId()
    {
        var request = new DocumentUploadRequestDto
        {
            Filename = "test.txt",
            EncodedFile = "SGVsbG8=", 
            ContentType = "text/plain",
            DocumentType = DocumentType.OTHER,
            Channel = Channel.DIGITAL
        };

        var result = await _service.UploadDocumentAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        _repositoryMock.Verify(r => r.AddAsync(It.Is<DocumentAsset>(d => 
            d.Filename == request.Filename && 
            d.Status == DocumentStatus.RECEIVED)), Times.Once);

        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<DocumentAsset>(), request.EncodedFile), Times.Once);
    }

    [Fact]
    public async Task SearchDocumentsAsync_ShouldReturnRepositoryResults()
    {
        var filters = new DocumentSearchFiltersDto { Filename = "test" };
        var expectedDocuments = new List<DocumentAsset> 
        { 
            new DocumentAsset { Filename = "test1.txt" },
            new DocumentAsset { Filename = "test2.txt" }
        };

        var expectedPagedResponse = new PagedResponseDto<DocumentAsset>(expectedDocuments, 2, 1, 10);

        _repositoryMock.Setup(r => r.SearchAsync(filters))
            .ReturnsAsync(expectedPagedResponse);

        var result = await _service.SearchDocumentsAsync(filters);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeEquivalentTo(expectedDocuments);
    }
}
