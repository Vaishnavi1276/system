// using Ardalis.GuardClauses;
// using AutoMapper;
// using BuildingBlocks.Abstractions.CQRS.Commands;
// using BuildingBlocks.Core.CQRS.Commands;
// using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
// using ECommerce.Services.Customers.Customers.Models.Reads;
// using ECommerce.Services.Customers.Shared.Data;
//
// namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
//
// public record CreateCustomerRead : InternalCommand
// {
//     public long CustomerId { get; init; }
//     public new Guid Id { get; init; }
//     public Guid IdentityId { get; init; }
//     public string Email { get; init; } = null!;
//     public string FirstName { get; init; } = null!;
//     public string LastName { get; init; } = null!;
//     public string FullName { get; init; } = null!;
//     public string? Country { get; init; }
//     public string? City { get; init; }
//     public string? DetailAddress { get; init; }
//     public string? Nationality { get; init; }
//     public DateTime? BirthDate { get; init; }
//     public string? PhoneNumber { get; init; }
//     public DateTime Created { get; init; }
// }
//
// internal class CreateCustomerReadHandler : ICommandHandler<CreateCustomerRead>
// {
//     private readonly IMapper _mapper;
//     private readonly CustomersReadDbContext _customersReadDbContext;
//
//     public CreateCustomerReadHandler(IMapper mapper, CustomersReadDbContext customersReadDbContext)
//     {
//         _mapper = mapper;
//         _customersReadDbContext = customersReadDbContext;
//     }
//
//     public async Task<Unit> Handle(CreateCustomerRead command, CancellationToken cancellationToken)
//     {
//         Guard.Against.Null(command, nameof(command));
//
//         var readModel = _mapper.Map<Customer>(command);
//
//         await _customersReadDbContext.Customers.InsertOneAsync(readModel, cancellationToken: cancellationToken);
//
//         return Unit.Value;
//     }
// }
//
using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Contracts;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;

public record CreateCustomerRead : InternalCommand
{
    public long CustomerId { get; init; }
    public new Guid Id { get; init; }
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
    public DateTime Created { get; init; }
}

internal class CreateCustomerReadHandler : ICommandHandler<CreateCustomerRead>
{
    private readonly IMapper _mapper;
    private readonly ICustomersReadUnitOfWork _unitOfWork;

    // totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
    // https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
    public CreateCustomerReadHandler(IMapper mapper, ICustomersReadUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateCustomerRead command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var readModel = _mapper.Map<Customer>(command);

        await _unitOfWork.CustomersRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
