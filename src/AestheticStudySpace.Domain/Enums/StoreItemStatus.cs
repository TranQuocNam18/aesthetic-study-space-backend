namespace AestheticStudySpace.Domain.Enums;

public enum StoreItemStatus
{
    /// <summary>Created directly by an Admin — always visible in store.</summary>
    AdminCreated = 0,

    /// <summary>Submitted by a user, awaiting Admin review.</summary>
    PendingReview = 1,

    /// <summary>Admin approved the user-submitted theme.</summary>
    Approved = 2,

    /// <summary>Admin rejected the user-submitted theme.</summary>
    Rejected = 3,

    /// <summary>User submitted, waiting for Admin to buy/approve transaction.</summary>
    PendingTransaction = 4,

    /// <summary>Admin bought the item, waiting for Admin to set the coin price and publish.</summary>
    PurchasedPendingPricing = 5
}
