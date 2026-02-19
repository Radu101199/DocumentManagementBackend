using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Application.UnitTests.TestHelpers;

public class DocumentBuilder
{
    private string _title = "Test Document";
    private string _description = "Test Description";
    private string _fileName = "test.pdf";
    private string _filePath = "/files/test.pdf";
    private string _contentType = "application/pdf";
    private long _fileSizeBytes = 1024;
    private Guid _ownerId = Guid.NewGuid();
    private Guid _creatorId = Guid.NewGuid();

    public static DocumentBuilder Create() => new();

    public DocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DocumentBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public Document Build()
    {
        return Document.Create(
            _title,
            _description,
            _fileName,
            _filePath,
            _contentType,
            _fileSizeBytes,
            _ownerId,
            _creatorId);
    }
}