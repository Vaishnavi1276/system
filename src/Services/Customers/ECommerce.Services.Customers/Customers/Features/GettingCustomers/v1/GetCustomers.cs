using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomers.v1;

public record GetCustomers : ListQuery<GetCustomersResult>;

public class GetCustomersValidator : AbstractValidator<GetCustomers>
{
    public GetCustomersValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

internal class GetCustomersHandler : IQueryHandler<GetCustomers, GetCustomersResult>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GetCustomersHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetCustomersResult> Handle(GetCustomers request, CancellationToken cancellationToken)
    {
        var customer = await _customersReadDbContext.Customers
            .AsQueryable()
            .OrderByDescending(x => x.City)
            .ApplyFilter(request.Filters)
            .ApplyPagingAsync<Customer, CustomerReadDto>(
                _mapper.ConfigurationProvider,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken
            );

        return new GetCustomersResult(customer);
    }
}

public record GetCustomersResult(IListResultModel<CustomerReadDto> Customers);
