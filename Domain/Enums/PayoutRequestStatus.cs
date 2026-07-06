namespace Domain.Enums;

public enum PayoutRequestStatus : short
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4,
    Cancelled = 5
}
