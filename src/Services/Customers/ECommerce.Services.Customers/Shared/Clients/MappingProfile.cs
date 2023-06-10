using AutoMapper;
using ECommerce.Services.Customers.Products.Models;
using ECommerce.Services.Customers.Shared.Clients.Catalogs.Dtos;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Services.Customers.Shared.Clients;

public class ClientsMappingProfile : Profile
{
    public ClientsMappingProfile()
    {
        CreateMap<ProductClientDto, Product>();
        CreateMap<IdentityUserClientDto, IdentityUser>();
    }
}
