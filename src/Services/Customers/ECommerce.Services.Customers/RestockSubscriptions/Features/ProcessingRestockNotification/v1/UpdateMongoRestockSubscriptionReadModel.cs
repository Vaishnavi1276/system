using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;
using RestockSubscription = ECommerce.Services.Customers.RestockSubscriptions.Models.Write.RestockSubscription;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;

public record UpdateMongoRestockSubscriptionReadModel(RestockSubscription RestockSubscription, bool IsDeleted)
    : InternalCommand;

internal class UpdateMongoRestockSubscriptionReadModelHandler : ICommandHandler<UpdateMongoRestockSubscriptionReadModel>
{
    private readonly CustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMongoRestockSubscriptionReadModelHandler(CustomersReadUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateMongoRestockSubscriptionReadModel command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var existingSubscription = await _unitOfWork.RestockSubscriptionsRepository.FindOneAsync(
            x => x.RestockSubscriptionId == command.RestockSubscription.Id,
            cancellationToken
        );

        if (existingSubscription is null)
        {
            throw new RestockSubscriptionNotFoundException(command.RestockSubscription.Id);
        }

        var updateSubscription = _mapper.Map(command.RestockSubscription, existingSubscription);

        await _unitOfWork.RestockSubscriptionsRepository.UpdateAsync(updateSubscription, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
