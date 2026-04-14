namespace SimilarProducts.Domain.Enums;

public enum UserRole
{
    Consumer,
    BusinessOwner,
    Admin
}

public enum BusinessOwnerStatus
{
    Pending,
    Approved,
    Rejected
}

public enum StoreType
{
    Grocery,
    Pharmacy,
    Organic,
    Supermarket
}

public enum OriginLevel
{
    TunisianMade,
    TunisianAssembled,
    Imported
}

public enum DataSource
{
    Manual,
    OpenFoodFacts,
    BrandDeclared
}

public enum ProductStatus
{
    Pending,
    Approved,
    External
}

public enum TagType
{
    Dietary,
    Attribute,
    Origin
}

public enum ThemeType
{
    System,
    Community,
    Event
}

public enum SimilaritySource
{
    BrandDeclared,
    StoreDeclared,
    AutoTags,
    Admin
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public enum RequestType
{
    BusinessRegistration,
    ProductAdd,
    Similarity,
    ThemeProposal
}

public enum MatchStatus
{
    Unmatched,
    AutoMatched,
    AdminMatched
}

public enum ScanFoundIn
{
    LocalDb,
    OpenFoodFacts,
    NotFound
}
