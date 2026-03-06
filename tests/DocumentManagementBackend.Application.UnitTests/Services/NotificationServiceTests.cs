using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Services;

[TestFixture]
public class NotificationServiceTests
{
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IDocumentRepository> _documentRepositoryMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<ILogger<NotificationService>> _loggerMock = null!;
    private NotificationService _service = null!;

    private User _testUser = null!;
    private Document _testDocument = null!;

    [SetUp]
    public void SetUp()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<NotificationService>>();

        _service = new NotificationService(
            _emailServiceMock.Object,
            _documentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);

        _testUser = User.Create(
            Email.Create("owner@test.com"),
            "John", "Doe",
            "hashedpassword",
            UserRole.User);

        _testDocument = Document.Create(
            "Test Document", "Description",
            "test.pdf", "/files/test.pdf",
            "application/pdf", 1024,
            _testUser.Id, _testUser.Id);
    }

    [Test]
    public async Task NotifyDocumentApprovedAsync_Should_Send_Email_To_Owner()
    {
        // Arrange
        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(_testDocument.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testDocument);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        await _service.NotifyDocumentApprovedAsync(
            _testDocument.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            It.Is<EmailMessage>(m =>
                m.To == "owner@test.com" &&
                m.Subject.Contains("approved")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task NotifyDocumentApprovedAsync_Should_Not_Send_Email_When_Document_NotFound()
    {
        // Arrange
        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        await _service.NotifyDocumentApprovedAsync(
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task NotifyDocumentRejectedAsync_Should_Send_Email_With_Reason()
    {
        // Arrange
        var reason = "Missing signatures";

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(_testDocument.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testDocument);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        await _service.NotifyDocumentRejectedAsync(
            _testDocument.Id, Guid.NewGuid(), reason, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            It.Is<EmailMessage>(m =>
                m.To == "owner@test.com" &&
                m.Subject.Contains("rejected") &&
                m.Body.Contains(reason)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task NotifyDocumentCreatedAsync_Should_Send_Email_To_Creator()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        await _service.NotifyDocumentCreatedAsync(
            _testDocument.Id, _testUser.Id, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            It.Is<EmailMessage>(m =>
                m.To == "owner@test.com" &&
                m.Subject.Contains("created")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task NotifyDocumentCreatedAsync_Should_Not_Send_Email_When_Creator_NotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _service.NotifyDocumentCreatedAsync(
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendAsync(
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}