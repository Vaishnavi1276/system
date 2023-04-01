using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Contracts;
using FluentValidation;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerById.v1;

public record GetCustomerById(Guid Id) : IQuery<GetCustomerByIdResult>
{
    internal class Validator : AbstractValidator<GetCustomerById>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

// totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
// https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
internal class GetCustomerByIdHandler : IQueryHandler<GetCustomerById, GetCustomerByIdResult>
{
    private readonly ICustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByIdHandler(ICustomersReadUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GetCustomerByIdResult> Handle(GetCustomerById query, CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var customer = await _unitOfWork.CustomersRepository.FindOneAsync(
            x => x.Id == query.Id,
            cancellationToken: cancellationToken
        );

        Guard.Against.NotFound(customer, new CustomerNotFoundException(query.Id));

        var customerDto = _mapper.Map<CustomerReadDto>(customer);

        return new GetCustomerByIdResult(customerDto);
    }
}

internal record GetCustomerByIdResult(CustomerReadDto Customer);
