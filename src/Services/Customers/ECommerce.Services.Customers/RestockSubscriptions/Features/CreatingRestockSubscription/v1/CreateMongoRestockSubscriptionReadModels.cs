using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public record CreateMongoRestockSubscriptionReadModels(
    long RestockSubscriptionId,
    long CustomerId,
    string CustomerName,
    long ProductId,
    string ProductName,
    string Email,
    DateTime Created,
    bool Processed,
    DateTime? ProcessedTime = null
) : InternalCommand
{
    public bool IsDeleted { get; init; }
}

internal class CreateRestockSubscriptionReadModelHandler : ICommandHandler<CreateMongoRestockSubscriptionReadModels>
{
    private readonly CustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRestockSubscriptionReadModelHandler(CustomersReadUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(
        CreateMongoRestockSubscriptionReadModels command,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(command, nameof(command));

        var readModel = _mapper.Map<RestockSubscription>(command);

        await _unitOfWork.RestockSubscriptionsRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
