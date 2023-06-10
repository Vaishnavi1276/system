using BuildingBlocks.Abstractions.Domain.Events;

namespace BuildingBlocks.Abstractions.Domain;

public interface IHaveAggregate : IHaveDomainEvents, IHaveAggregateVersion { }
