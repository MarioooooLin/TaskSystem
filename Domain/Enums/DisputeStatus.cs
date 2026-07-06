namespace Domain.Enums;

public enum DisputeStatus : short
{
    Open = 1,
    UnderReview = 2,
    ResolvedForMerchant = 3,
    ResolvedForKol = 4,
    ResolvedCompromise = 5,
    Cancelled = 6
}
