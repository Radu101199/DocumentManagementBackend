using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Events;
using DocumentManagementBackend.Domain.Exceptions;

namespace DocumentManagementBackend.Domain.UnitTests.TestHelpers;

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

    public DocumentBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public DocumentBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DocumentBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public DocumentBuilder WithCreatorId(Guid creatorId)
    {
        _creatorId = creatorId;
        return this;
    }

    public DocumentBuilder WithFileSizeBytes(long fileSizeBytes)
    {
        _fileSizeBytes = fileSizeBytes;
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

    // Helpers pentru stări specifice
    public Document BuildInReview(Guid? reviewerId = null)
    {
        var document = Build();
        document.RequestApproval();
        document.MarkAsReviewed(reviewerId ?? Guid.NewGuid());
        return document;
    }

    public Document BuildApproved(Guid? approverId = null)
    {
        var document = Build();
        document.RequestApproval();
        document.Approve(approverId ?? Guid.NewGuid());
        return document;
    }

    public Document BuildRejected(Guid? rejectorId = null)
    {
        var document = Build();
        document.RequestApproval();
        document.Reject(rejectorId ?? Guid.NewGuid(), "Test rejection reason");
        return document;
    }

    public Document BuildArchived()
    {
        var document = Build();
        document.Archive();
        return document;
    }
}