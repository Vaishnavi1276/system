using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Customers.Customers.Data.UOW.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionsByEmails.v1;

public record GetRestockSubscriptionsByEmails(IList<string> Emails) : IStreamQuery<RestockSubscriptionDto>;

internal class GetRestockSubscriptionsByEmailsValidator : AbstractValidator<GetRestockSubscriptionsByEmails>
{
    public GetRestockSubscriptionsByEmailsValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Emails).NotNull().NotEmpty();
    }
}

internal class GetRestockSubscriptionsByEmailsHandler
    : IStreamQueryHandler<GetRestockSubscriptionsByEmails, RestockSubscriptionDto>
{
    private readonly CustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRestockSubscriptionsByEmailsHandler(CustomersReadUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public IAsyncEnumerable<RestockSubscriptionDto> Handle(
        GetRestockSubscriptionsByEmails query,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(query, nameof(query));

        var result = _unitOfWork.RestockSubscriptionsRepository.ProjectBy<RestockSubscriptionDto, Guid>(
            _mapper.ConfigurationProvider,
            x => !x.IsDeleted && query.Emails.Contains(x.Email!),
            x => x.Id,
            cancellationToken
        );

        return result;
    }
}
