using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Identity.Shared.Extensions;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Dtos.v1;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Sieve.Services;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers.v1;

public record GetUsers : PageQuery<GetUsersResponse>;

public class GetUsersValidator : AbstractValidator<GetUsers>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _userManager = userManager;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async Task<GetUsersResponse> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        var customer = await _userManager.FindAllUsersByPageAsync<IdentityUserDto>(
            request,
            _mapper,
            _sieveProcessor,
            cancellationToken
        );

        return new GetUsersResponse(customer);
    }
}
