using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Events;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.UnitTests.TestHelpers;
using NUnit.Framework;

namespace DocumentManagementBackend.Domain.UnitTests.Entities;

public class DocumentTests
{
    // ============================================================
    // CREATE
    // ============================================================
    public class Create
    {
        [Test]
        public void Should_Create_Document_With_Default_Draft_Status()
        {
            // Act
            var document = DocumentBuilder.Create().Build();

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Draft));
        }

        [Test]
        public void Should_Set_Initial_State_Correctly()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();

            // Act
            var document = DocumentBuilder.Create()
                .WithTitle("My Document")
                .WithFileName("my_doc.pdf")
                .WithOwnerId(ownerId)
                .WithCreatorId(creatorId)
                .Build();

            // Assert
            Assert.That(document.Title, Is.EqualTo("My Document"));
            Assert.That(document.FileName, Is.EqualTo("my_doc.pdf"));
            Assert.That(document.OwnerId, Is.EqualTo(ownerId));
            Assert.That(document.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void Should_Raise_DocumentCreated_Event()
        {
            // Arrange
            var creatorId = Guid.NewGuid();

            // Act
            var document = DocumentBuilder.Create()
                .WithCreatorId(creatorId)
                .Build();

            // Assert
            Assert.That(document.DomainEvents, Has.Count.EqualTo(1));
            Assert.That(document.DomainEvents.First(), Is.InstanceOf<DocumentCreatedEvent>());

            var @event = (DocumentCreatedEvent)document.DomainEvents.First();
            Assert.That(@event.ActorId, Is.EqualTo(creatorId));
            Assert.That(@event.DocumentId, Is.EqualTo(document.Id));
        }

        [Test]
        public void Should_Throw_When_Title_IsEmpty()
        {
            Assert.Throws<DomainException>(() =>
                DocumentBuilder.Create().WithTitle("").Build());
        }

        [Test]
        public void Should_Throw_When_FileName_IsEmpty()
        {
            Assert.Throws<DomainException>(() =>
                DocumentBuilder.Create().WithFileName("").Build());
        }

        [Test]
        public void Should_Throw_When_FileSizeBytes_IsZero()
        {
            Assert.Throws<DomainException>(() =>
                DocumentBuilder.Create().WithFileSizeBytes(0).Build());
        }

        [Test]
        public void Should_Throw_When_FileSizeBytes_IsNegative()
        {
            Assert.Throws<DomainException>(() =>
                DocumentBuilder.Create().WithFileSizeBytes(-1).Build());
        }
    }

    // ============================================================
    // MARK AS REVIEWED
    // ============================================================
    public class MarkReviewed
    {
        [Test]
        public void Should_Raise_DocumentReviewed_Event()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();
            var reviewerId = Guid.NewGuid();

            // Act
            document.MarkAsReviewed(reviewerId, "Looks good");

            // Assert
            var reviewEvent = document.DomainEvents
                .OfType<DocumentReviewedEvent>()
                .FirstOrDefault();

            Assert.That(reviewEvent, Is.Not.Null);
            Assert.That(reviewEvent!.ReviewerId, Is.EqualTo(reviewerId));
        }

        [Test]
        public void Should_Throw_When_Document_Is_Archived()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildArchived();

            // Act & Assert
            Assert.Throws<DocumentInvalidStateException>(() =>
                document.MarkAsReviewed(Guid.NewGuid()));
        }

        [Test]
        public void Should_Include_Notes_In_Event()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();
            const string notes = "Please check section 3";

            // Act
            document.MarkAsReviewed(Guid.NewGuid(), notes);

            // Assert
            var reviewEvent = document.DomainEvents
                .OfType<DocumentReviewedEvent>()
                .First();

            Assert.That(reviewEvent.ReviewNotes, Is.EqualTo(notes));
        }
    }

    // ============================================================
    // APPROVE
    // ============================================================
    public class Approve
    {
        [Test]
        public void Should_Approve_When_In_Draft()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act
            document.Approve(Guid.NewGuid());

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Published));
        }

        [Test]
        public void Should_Raise_DocumentApproved_Event()
        {
            // Arrange
            var approverId = Guid.NewGuid();
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act
            document.Approve(approverId);

            // Assert
            var approvedEvent = document.DomainEvents
                .OfType<DocumentApprovedEvent>()
                .FirstOrDefault();

            Assert.That(approvedEvent, Is.Not.Null);
            Assert.That(approvedEvent!.ApproverId, Is.EqualTo(approverId));
            Assert.That(approvedEvent.DocumentId, Is.EqualTo(document.Id));
        }

        [Test]
        public void Should_Throw_When_Already_Approved()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildApproved();

            // Act & Assert
            Assert.Throws<InvalidStatusTransitionException>(() =>
                document.Approve(Guid.NewGuid()));
        }

        [Test]
        public void Should_Throw_When_Document_Is_Archived()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildArchived();

            // Act & Assert
            Assert.Throws<DocumentInvalidStateException>(() =>
                document.Approve(Guid.NewGuid()));
        }

        [Test]
        public void Should_Throw_When_Document_Is_Rejected()
        {
            // Arrange - după reject, documentul e Draft dar fără approval window
            var document = DocumentBuilder.Create().BuildRejected();

            // Act & Assert - nu poți aproba fără să fi requestat din nou approval
            // ApprovalExpiresAt e null, deci nu are approval window activ
            Assert.That(document.ApprovalRequestedAt, Is.Null);
            Assert.That(document.ApprovalExpiresAt, Is.Null);
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Draft));
        }
    }

    // ============================================================
    // REJECT
    // ============================================================
    public class Reject
    {
        [Test]
        public void Should_Reject_When_In_Draft()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act
            document.Reject(Guid.NewGuid(), "Missing signature");

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Draft));
        }

        [Test]
        public void Should_Raise_DocumentRejected_Event()
        {
            // Arrange
            var rejectorId = Guid.NewGuid();
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act
            document.Reject(rejectorId, "Missing signature");

            // Assert
            var rejectedEvent = document.DomainEvents
                .OfType<DocumentRejectedEvent>()
                .FirstOrDefault();

            Assert.That(rejectedEvent, Is.Not.Null);
            Assert.That(rejectedEvent!.RejectorId, Is.EqualTo(rejectorId));
            Assert.That(rejectedEvent.RejectionReason, Is.EqualTo("Missing signature"));
        }

        [Test]
        public void Should_Clear_ApprovalWindow_After_Rejection()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act
            document.Reject(Guid.NewGuid(), "Missing signature");

            // Assert
            Assert.That(document.ApprovalRequestedAt, Is.Null);
            Assert.That(document.ApprovalExpiresAt, Is.Null);
        }

        [Test]
        public void Should_Throw_When_Rejection_Reason_IsEmpty()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                document.Reject(Guid.NewGuid(), ""));
        }

        [Test]
        public void Should_Throw_When_Already_Published()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildApproved();

            // Act & Assert
            Assert.Throws<InvalidStatusTransitionException>(() =>
                document.Reject(Guid.NewGuid(), "Some reason"));
        }

        [Test]
        public void Should_Throw_When_Document_Is_Archived()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildArchived();

            var ex = Assert.Throws<DocumentInvalidStateException>(
                () => document.Reject(Guid.NewGuid(), "Some reason"));

            Assert.That(ex.Message, Does.Contain("Archived"));
        }
    }

    // ============================================================
    // CANCEL APPROVAL
    // ============================================================
    public class CancelApproval
    {
        [Test]
        public void Should_Cancel_When_Published()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildApproved();

            // Act
            document.CancelApproval(Guid.NewGuid());

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Draft));
        }

        [Test]
        public void Should_Raise_DocumentApprovalCancelled_Event()
        {
            // Arrange
            var cancelledById = Guid.NewGuid();
            var document = DocumentBuilder.Create().BuildApproved();

            // Act
            document.CancelApproval(cancelledById, "Wrong version");

            // Assert
            var cancelEvent = document.DomainEvents
                .OfType<DocumentApprovalCancelledEvent>()
                .FirstOrDefault();

            Assert.That(cancelEvent, Is.Not.Null);
            Assert.That(cancelEvent!.CancelledById, Is.EqualTo(cancelledById));
            Assert.That(cancelEvent.CancellationReason, Is.EqualTo("Wrong version"));
        }

        [Test]
        public void Should_Clear_ApprovalWindow_After_Cancel()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildApproved();

            // Act
            document.CancelApproval(Guid.NewGuid());

            // Assert
            Assert.That(document.ApprovalRequestedAt, Is.Null);
            Assert.That(document.ApprovalExpiresAt, Is.Null);
        }

        [Test]
        public void Should_Throw_When_Not_Published()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();

            // Act & Assert
            Assert.Throws<DocumentInvalidStateException>(() =>
                document.CancelApproval(Guid.NewGuid()));
        }

        [Test]
        public void Should_Throw_When_Document_Is_Archived()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildArchived();

            // Act & Assert
            Assert.Throws<DocumentInvalidStateException>(() =>
                document.CancelApproval(Guid.NewGuid()));
        }
    }

    // ============================================================
    // ARCHIVE
    // ============================================================
    public class Archive
    {
        [Test]
        public void Should_Archive_From_Draft()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();

            // Act
            document.Archive();

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Archived));
        }

        [Test]
        public void Should_Archive_From_Published()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildApproved();

            // Act
            document.Archive();

            // Assert
            Assert.That(document.Status, Is.EqualTo(DocumentStatus.Archived));
        }

        [Test]
        public void Should_Throw_When_Already_Archived()
        {
            // Arrange
            var document = DocumentBuilder.Create().BuildArchived();

            // Act & Assert
            Assert.Throws<DocumentInvalidStateException>(() =>
                document.Archive());
        }
    }

    // ============================================================
    // DOMAIN EVENTS
    // ============================================================
    public class DomainEvents
    {
        [Test]
        public void Should_Clear_Events_After_ClearDomainEvents()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            Assert.That(document.DomainEvents, Has.Count.GreaterThan(0));

            // Act
            document.ClearDomainEvents();

            // Assert
            Assert.That(document.DomainEvents, Is.Empty);
        }

        [Test]
        public void Should_Accumulate_Multiple_Events()
        {
            // Arrange
            var document = DocumentBuilder.Create().Build();
            document.RequestApproval();
            document.MarkAsReviewed(Guid.NewGuid());
            document.Approve(Guid.NewGuid());

            // Assert
            Assert.That(document.DomainEvents, Has.Count.EqualTo(3));
            Assert.That(document.DomainEvents.Any(e => e is DocumentCreatedEvent));
            Assert.That(document.DomainEvents.Any(e => e is DocumentReviewedEvent));
            Assert.That(document.DomainEvents.Any(e => e is DocumentApprovedEvent));
        }
    }
}