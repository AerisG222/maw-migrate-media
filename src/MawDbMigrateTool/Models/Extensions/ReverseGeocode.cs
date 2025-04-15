using MawDbMigrateTool.Models.Source;
using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool.Models.Extensions;

public static class ReverseGeocodeExtensions
{
    public static Location ToTarget(
        this PhotoReverseGeocode reverseGeocode,
        Guid id,
        double latitude,
        double longitude
    ) {
        return new Location
        {
            Id = id,
            Latitude = latitude,
            Longitude = longitude,
            LookupDate = DateTime.MinValue,
            FormattedAddress = reverseGeocode.FormattedAddress,
            AdministrativeAreaLevel1 = reverseGeocode.AdministrativeAreaLevel1,
            AdministrativeAreaLevel2 = reverseGeocode.AdministrativeAreaLevel2,
            AdministrativeAreaLevel3 = reverseGeocode.AdministrativeAreaLevel3,
            Country = reverseGeocode.Country,
            Locality = reverseGeocode.Locality,
            Neighborhood = reverseGeocode.Neighborhood,
            SubLocalityLevel1 = reverseGeocode.SubLocalityLevel1,
            SubLocalityLevel2 = reverseGeocode.SubLocalityLevel2,
            PostalCode = reverseGeocode.PostalCode,
            PostalCodeSuffix = reverseGeocode.PostalCodeSuffix,
            Premise = reverseGeocode.Premise,
            Route = reverseGeocode.Route,
            StreetNumber = reverseGeocode.StreetNumber,
            SubPremise = reverseGeocode.SubPremise
        };
    }

    public static Location ToTarget(
        this VideoReverseGeocode reverseGeocode,
        Guid id,
        double latitude,
        double longitude
    ) {
        return new Location
        {
            Id = id,
            Latitude = latitude,
            Longitude = longitude,
            LookupDate = DateTime.MinValue,
            FormattedAddress = reverseGeocode.FormattedAddress,
            AdministrativeAreaLevel1 = reverseGeocode.AdministrativeAreaLevel1,
            AdministrativeAreaLevel2 = reverseGeocode.AdministrativeAreaLevel2,
            AdministrativeAreaLevel3 = reverseGeocode.AdministrativeAreaLevel3,
            Country = reverseGeocode.Country,
            Locality = reverseGeocode.Locality,
            Neighborhood = reverseGeocode.Neighborhood,
            SubLocalityLevel1 = reverseGeocode.SubLocalityLevel1,
            SubLocalityLevel2 = reverseGeocode.SubLocalityLevel2,
            PostalCode = reverseGeocode.PostalCode,
            PostalCodeSuffix = reverseGeocode.PostalCodeSuffix,
            Premise = reverseGeocode.Premise,
            Route = reverseGeocode.Route,
            StreetNumber = reverseGeocode.StreetNumber,
            SubPremise = reverseGeocode.SubPremise
        };
    }
}