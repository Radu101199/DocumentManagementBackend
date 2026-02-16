namespace DocumentManagementBackend.Domain.Enums;

public enum DocumentStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Published = 5,
    Archived = 6
}