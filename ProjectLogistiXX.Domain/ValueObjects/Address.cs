using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.SharedKernel;
using System.Net.Http.Headers;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public sealed record Address
    {
        public string? Street { get; }
        public string? StreetNumber { get; }
        public string? BuildingDetails { get; }
        public string? City { get; }
        public string? County { get; }
        public string? PostalCode { get; }
        public string? Country { get; }

        public string DisplayValue => $"{Street} {StreetNumber}, {City}, {Country}";

        private Address()
        {
        }
        internal Address(
            string? street,
            string? streetNumber,
            string? buildingDetails,
            string? city,
            string? county,
            string? postalCode,
            string? country)
        {
            Street = street;
            StreetNumber = streetNumber;
            BuildingDetails = buildingDetails;
            City = city;
            County = county;
            PostalCode = postalCode;
            Country = country;
        }

        public static Result<Address> Create(
            string? street,
            string? streetNumber,
            string? buildingDetails,
            string? city,
            string? county,
            string? postalCode,
            string? country)
        {

            var errors = ValidateError(street, city, county, country);

            if (errors.Count > 0)
            {
                return Result<Address>.Failure(errors);
            }

            var address = new Address(
                street!.Trim(),
                streetNumber?.Trim(),
                buildingDetails?.Trim(),
                city!.Trim(),
                county!.Trim(),
                postalCode?.Trim(),
                country!.Trim());

            return Result<Address>.Success(address);
        }

        private static List<Error> ValidateError(
            string? street,
            string? city,
            string? county,
            string? country)
        {

            var erros = new List<Error>();

            if (string.IsNullOrWhiteSpace(street))
            {
                erros.Add(Error.Validation("InvalidStreet", "Street is required."));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                erros.Add(Error.Validation("InvalidCity", "City is required."));
            }

            if (string.IsNullOrWhiteSpace(county))
            {
                erros.Add(Error.Validation("InvalidCounty", "County is required."));
            }

            if (string.IsNullOrWhiteSpace(country))
            {
                erros.Add(Error.Validation("InvalidCountry", "Country is required."));
            }

            return erros;
        }

    }
}
