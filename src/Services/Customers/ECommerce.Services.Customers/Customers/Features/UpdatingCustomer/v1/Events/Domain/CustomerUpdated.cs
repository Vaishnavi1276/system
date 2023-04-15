using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.Read.Mongo;
using FluentValidation;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;

public record CustomerUpdated(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid IdentityId,
    DateTime CreatedAt,
    DateTime? BirthDate = null,
    string? Country = null,
    string? City = null,
    string? DetailAddress = null,
    string? Nationality = null
) : DomainEvent
{
    public static CustomerUpdated Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        Guid identityId,
        DateTime createdAt,
        DateTime? birthDate,
        string? country = null,
        string? city = null,
        string? detailAddress = null,
        string? nationality = null
    )
    {
        return new CustomerUpdatedValidator().HandleValidation(
            new CustomerUpdated(
                id,
                firstName!,
                lastName!,
                email!,
                phoneNumber!,
                identityId,
                createdAt,
                birthDate,
                country,
                city,
                detailAddress,
                nationality
            )
        );
    }
}

internal class CustomerUpdatedValidator : AbstractValidator<CustomerUpdated>
{
    public CustomerUpdatedValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IdentityId).NotEmpty();
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

internal class CustomerCreatedHandler : IDomainEventHandler<CustomerUpdated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IMapper _mapper;

    public CustomerCreatedHandler(ICommandProcessor commandProcessor, IMapper mapper)
    {
        _commandProcessor = commandProcessor;
        _mapper = mapper;
    }

    public Task Handle(CustomerUpdated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();
        var mongoReadCommand = _mapper.Map<UpdateCustomerRead>(notification);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        return _commandProcessor.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
