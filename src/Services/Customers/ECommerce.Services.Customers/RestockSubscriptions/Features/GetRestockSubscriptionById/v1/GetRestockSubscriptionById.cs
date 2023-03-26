using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GetRestockSubscriptionById.v1;

public record GetRestockSubscriptionById(Guid Id) : IQuery<GetRestockSubscriptionByIdResponse>
{
    internal class Validator : AbstractValidator<GetRestockSubscriptionById>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }

        internal class Handler : IQueryHandler<GetRestockSubscriptionById, GetRestockSubscriptionByIdResponse>
        {
            private readonly CustomersReadUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public Handler(CustomersReadUnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<GetRestockSubscriptionByIdResponse> Handle(
                GetRestockSubscriptionById query,
                CancellationToken cancellationToken
            )
            {
                Guard.Against.Null(query, nameof(query));

                var restockSubscription = await _unitOfWork.RestockSubscriptionsRepository.FindOneAsync(
                    x => x.IsDeleted == false && x.Id == query.Id,
                    cancellationToken
                );

                Guard.Against.NotFound(restockSubscription, new RestockSubscriptionNotFoundException(query.Id));

                var subscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

                return new GetRestockSubscriptionByIdResponse(subscriptionDto);
            }
        }
    }
}
