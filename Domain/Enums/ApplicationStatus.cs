namespace Domain.Enums;

public enum ApplicationStatus : short
{
    Applied = 1,
    Accepted = 2,
    PendingReconfirmation = 3,
    Rejected = 4,
    Cancelled = 5,
    Invalid = 6
}
