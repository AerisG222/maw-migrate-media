namespace MawDbMigrate.Models.Target;

public class Location
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LookupDate { get; set; }
    public string? FormattedAddress { get; set; }
    public string? AdministrativeAreaLevel1 { get; set; }
    public string? AdministrativeAreaLevel2 { get; set; }
    public string? AdministrativeAreaLevel3 { get; set; }
    public string? Country { get; set; }
    public string? Locality { get; set; }
    public string? Neighborhood { get; set; }
    public string? SubLocalityLevel1 { get; set; }
    public string? SubLocalityLevel2 { get; set; }
    public string? PostalCode { get; set; }
    public string? PostalCodeSuffix { get; set; }
    public string? Premise { get; set; }
    public string? Route { get; set; }
    public string? StreetNumber { get; set; }
    public string? SubPremise { get; set; }
}