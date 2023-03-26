using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.Read.Mongo;

public record UpdateCustomerRead : InternalCommand
{
    public new Guid Id { get; init; }
    public long CustomerId { get; init; }
    public Guid IdentityId { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? DetailAddress { get; init; }
    public string? Nationality { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? PhoneNumber { get; init; }
}

internal class UpdateCustomerReadHandler : ICommandHandler<UpdateCustomerRead>
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;
    private readonly IMapper _mapper;

    // totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
    // https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
    public UpdateCustomerReadHandler(ICustomersReadUnitOfWork customersReadUnitOfWork, IMapper mapper)
    {
        _customersReadUnitOfWork = customersReadUnitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateCustomerRead command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var existingCustomer = await _customersReadUnitOfWork.CustomersRepository.FindOneAsync(
            x => x.CustomerId == command.CustomerId,
            cancellationToken
        );

        if (existingCustomer is null)
        {
            throw new CustomerNotFoundException(command.CustomerId);
        }

        var updateCustomer = _mapper.Map(command, existingCustomer);

        await _customersReadUnitOfWork.CustomersRepository.UpdateAsync(updateCustomer, cancellationToken);

        await _customersReadUnitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
