namespace EPR.Common.Authorization.Models;

public class Organisation
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    public string? TradingName { get; set; }

    public string? OrganisationRole { get; set; }

    public string? OrganisationType { get; set; }

    public string? OrganisationNumber { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    public int? NationId { get; set; }

    public string? SubBuildingName { get; set; }

    public string? BuildingName { get; set; }

    public string? BuildingNumber { get; set; }

    public string? Street { get; set; }

    public string? Locality { get; set; }

    public string? DependentLocality { get; set; }

    public string? Town { get; set; }

    public string? County { get; set; }

    public string? Country { get; set; }

    public string? Postcode { get; set; }

    public string? JobTitle { get; set; }
}