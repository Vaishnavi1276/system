using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.ValueObjects;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;

public sealed record UpdateCustomer(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid IdentityId,
    DateTime? BirthDate = null,
    string? Country = null,
    string? City = null,
    string? DetailAddress = null,
    string? Nationality = null
) : ICommand
{
    public static UpdateCustomer Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        Guid identityId,
        DateTime? birthDate = null,
        string? country = null,
        string? city = null,
        string? detailAddress = null,
        string? nationality = null
    )
    {
        return new UpdateCustomerValidator().HandleValidation(
            new UpdateCustomer(
                id,
                firstName!,
                lastName!,
                email!,
                phoneNumber!,
                identityId,
                birthDate,
                country,
                city,
                detailAddress,
                nationality
            )
        );
    }
}

internal class UpdateCustomerValidator : AbstractValidator<UpdateCustomer>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(p => p.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone Number is required.")
            .MinimumLength(7)
            .WithMessage("PhoneNumber must not be less than 7 characters.")
            .MaximumLength(15)
            .WithMessage("PhoneNumber must not exceed 15 characters.");
    }
}

internal class UpdateCustomerHandler : ICommandHandler<UpdateCustomer>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(CustomersDbContext customersDbContext, ILogger<UpdateCustomerHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateCustomer command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer");

        command.NotBeNull();

        var customer = await _customersDbContext.Customers.FindAsync(
            new object?[] { CustomerId.Of(command.Id) },
            cancellationToken: cancellationToken
        );

        if (customer is null)
        {
            throw new CustomerNotFoundException(command.Id);
        }

        customer.Update(
            Email.Of(command.Email),
            PhoneNumber.Of(command.PhoneNumber),
            CustomerName.Of(command.FirstName, command.LastName),
            null,
            command.BirthDate == null ? null : BirthDate.Of((DateTime)command.BirthDate),
            command.Nationality == null ? null : Nationality.Of(command.Nationality)
        );

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer with Id: '{@CustomerId}' updated.", customer.Id);

        // TODO: Update Identity user with new customer changes
        return Unit.Value;
    }
}
