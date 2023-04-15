using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;

public record CustomerUpdatedV1(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid IdentityId,
    DateTime CreatedAt,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Address = null
) : IntegrationEvent
{
    public static CustomerUpdatedV1 Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        Guid identityId,
        DateTime createdAt,
        DateTime? birthDate,
        string? nationality,
        string? address
    )
    {
        id.NotBeNegativeOrZero();
        firstName.NotBeNullOrWhiteSpace();
        lastName.NotBeNullOrWhiteSpace();
        email.NotBeNullOrWhiteSpace().NotBeInvalidEmail();
        phoneNumber.NotBeNullOrWhiteSpace();
        identityId.NotBeEmpty();

        return new CustomerUpdatedV1(
            id,
            firstName,
            lastName,
            email,
            phoneNumber,
            identityId,
            createdAt,
            birthDate,
            nationality,
            address
        );
    }
}
