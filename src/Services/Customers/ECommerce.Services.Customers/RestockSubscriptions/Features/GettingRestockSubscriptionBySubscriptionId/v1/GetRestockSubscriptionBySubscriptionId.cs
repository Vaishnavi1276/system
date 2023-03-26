using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionBySubscriptionId.v1;

public record GetRestockSubscriptionBySubscriptionId(long RestockSubscriptionId)
    : IQuery<GetRestockSubscriptionBySubscriptionIdResponse>
{
    internal class Validator : AbstractValidator<GetRestockSubscriptionBySubscriptionId>
    {
        public Validator()
        {
            RuleFor(x => x.RestockSubscriptionId).NotEmpty();
        }

        internal class Handler
            : IQueryHandler<GetRestockSubscriptionBySubscriptionId, GetRestockSubscriptionBySubscriptionIdResponse>
        {
            private readonly CustomersReadUnitOfWork _customersReadUnitOfWork;
            private readonly IMapper _mapper;

            public Handler(CustomersReadUnitOfWork customersReadUnitOfWork, IMapper mapper)
            {
                _customersReadUnitOfWork = customersReadUnitOfWork;
                _mapper = mapper;
            }

            public async Task<GetRestockSubscriptionBySubscriptionIdResponse> Handle(
                GetRestockSubscriptionBySubscriptionId query,
                CancellationToken cancellationToken
            )
            {
                Guard.Against.Null(query, nameof(query));

                var restockSubscription = await _customersReadUnitOfWork.RestockSubscriptionsRepository.FindOneAsync(
                    x => x.IsDeleted == false && x.RestockSubscriptionId == query.RestockSubscriptionId,
                    cancellationToken
                );

                Guard.Against.NotFound(
                    restockSubscription,
                    new RestockSubscriptionNotFoundException(query.RestockSubscriptionId)
                );

                var subscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

                return new GetRestockSubscriptionBySubscriptionIdResponse(subscriptionDto);
            }
        }
    }
}
