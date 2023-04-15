using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

public record GetRestockSubscriptions : PageQuery<GetRestockSubscriptionsResponse>
{
    public IList<string> Emails { get; init; } = null!;
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

internal class GetRestockSubscriptionsValidator : AbstractValidator<GetRestockSubscriptions>
{
    public GetRestockSubscriptionsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GeRestockSubscriptionsHandler : IQueryHandler<GetRestockSubscriptions, GetRestockSubscriptionsResponse>
{
    private readonly CustomersReadUnitOfWork _customersReadUnitOfWork;
    private readonly IMapper _mapper;

    public GeRestockSubscriptionsHandler(CustomersReadUnitOfWork customersReadUnitOfWork, IMapper mapper)
    {
        _customersReadUnitOfWork = customersReadUnitOfWork;
        _mapper = mapper;
    }

    public async Task<GetRestockSubscriptionsResponse> Handle(
        GetRestockSubscriptions query,
        CancellationToken cancellationToken
    )
    {
        var restockSubscriptions = await _customersReadUnitOfWork.RestockSubscriptionsRepository.GetByPageFilter<
            RestockSubscriptionDto,
            DateTime
        >(
            query,
            _mapper.ConfigurationProvider,
            x =>
                x.IsDeleted == false
                && (query.Emails.Any() == false || query.Emails.Contains(x.Email))
                && (
                    (query.From == null && query.To == null)
                    || (query.From == null && x.Created <= query.To)
                    || (query.To == null && x.Created >= query.From)
                    || (x.Created >= query.From && x.Created <= query.To)
                ),
            sortExpression: x => x.Created,
            cancellationToken: cancellationToken
        );

        return new GetRestockSubscriptionsResponse(restockSubscriptions);
    }
}
