using MawDbMigrate.Models.Source;
using MawDbMigrate.Models.Target;

namespace MawDbMigrate.Models.Extensions;

public static class ReverseGeocodeExtensions
{
    public static void Populate(
        this PhotoReverseGeocode reverseGeocode,
        Location location
    ) {
        location.LookupDate = DateTime.MinValue;
        location.FormattedAddress = reverseGeocode.FormattedAddress;
        location.AdministrativeAreaLevel1 = reverseGeocode.AdministrativeAreaLevel1;
        location.AdministrativeAreaLevel2 = reverseGeocode.AdministrativeAreaLevel2;
        location.AdministrativeAreaLevel3 = reverseGeocode.AdministrativeAreaLevel3;
        location.Country = reverseGeocode.Country;
        location.Locality = reverseGeocode.Locality;
        location.Neighborhood = reverseGeocode.Neighborhood;
        location.SubLocalityLevel1 = reverseGeocode.SubLocalityLevel1;
        location.SubLocalityLevel2 = reverseGeocode.SubLocalityLevel2;
        location.PostalCode = reverseGeocode.PostalCode;
        location.PostalCodeSuffix = reverseGeocode.PostalCodeSuffix;
        location.Premise = reverseGeocode.Premise;
        location.Route = reverseGeocode.Route;
        location.StreetNumber = reverseGeocode.StreetNumber;
        location.SubPremise = reverseGeocode.SubPremise;
    }

    public static void Populate(
        this VideoReverseGeocode reverseGeocode,
        Location location
    ) {
        location.LookupDate = DateTime.MinValue;
        location.FormattedAddress = reverseGeocode.FormattedAddress;
        location.AdministrativeAreaLevel1 = reverseGeocode.AdministrativeAreaLevel1;
        location.AdministrativeAreaLevel2 = reverseGeocode.AdministrativeAreaLevel2;
        location.AdministrativeAreaLevel3 = reverseGeocode.AdministrativeAreaLevel3;
        location.Country = reverseGeocode.Country;
        location.Locality = reverseGeocode.Locality;
        location.Neighborhood = reverseGeocode.Neighborhood;
        location.SubLocalityLevel1 = reverseGeocode.SubLocalityLevel1;
        location.SubLocalityLevel2 = reverseGeocode.SubLocalityLevel2;
        location.PostalCode = reverseGeocode.PostalCode;
        location.PostalCodeSuffix = reverseGeocode.PostalCodeSuffix;
        location.Premise = reverseGeocode.Premise;
        location.Route = reverseGeocode.Route;
        location.StreetNumber = reverseGeocode.StreetNumber;
        location.SubPremise = reverseGeocode.SubPremise;
    }
}