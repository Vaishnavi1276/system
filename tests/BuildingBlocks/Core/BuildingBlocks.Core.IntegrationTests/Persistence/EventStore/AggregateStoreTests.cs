using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.EventSourcing;
using BuildingBlocks.Core.Persistence.EventStore;
using FluentAssertions;
using Tests.Shared.TestBase;
using Xunit.Abstractions;

namespace BuildingBlocks.Core.IntegrationTests.Persistence.EventStore;

public class AggregateStoreTests : IntegrationTest<>
{
    public AggregateStoreTests(IntegrationFixture integrationFixture, ITestOutputHelper outputHelper)
        : base(integrationFixture, outputHelper) { }

    [Fact]
    public async Task exist_stream_should_return_true_for_existing_stream()
    {
        var shoppingCart = await AddInitItemToStore();

        var exists = await AggregateStore.Exists<ShoppingCart, Guid>(shoppingCart.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task store_and_get_async_should_return_correct_aggregate()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());

        var appendResult = await AggregateStore.StoreAsync<ShoppingCart, Guid>(
            shoppingCart,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion),
            CancellationToken.None
        );

        var fetchedItem = await AggregateStore.GetAsync<ShoppingCart, Guid>(shoppingCart.Id);

        appendResult.Should().NotBeNull();
        appendResult.NextExpectedVersion.Should().Be(1);

        fetchedItem.Should().NotBeNull();
        fetchedItem!.Id.Should().Be(shoppingCart.Id);
        fetchedItem.Products.Should().NotBeNullOrEmpty();
        fetchedItem.Products.Count.Should().Be(shoppingCart.Products.Count);
    }

    private async Task<ShoppingCart> AddInitItemToStore()
    {
        var shoppingCart = ShoppingCart.Create(Guid.NewGuid());
        shoppingCart.AddItem(Guid.NewGuid());

        var appendResult = await AggregateStore.StoreAsync<ShoppingCart, Guid>(
            shoppingCart,
            new ExpectedStreamVersion(shoppingCart.OriginalVersion),
            CancellationToken.None
        );

        return shoppingCart;
    }

    private enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    private record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    private record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    private class ShoppingCart : EventSourcedAggregate<Guid>
    {
        private List<Guid> _products = new();

        public Guid ClientId { get; private set; }
        public ShoppingCartStatus Status { get; private set; }
        public IReadOnlyList<Guid> Products => _products.AsReadOnly();
        public DateTime? ConfirmedAt { get; private set; }

        public static ShoppingCart Create(Guid clientId)
        {
            var shoppingCart = new ShoppingCart();

            shoppingCart.ApplyEvent(new ShoppingCartInitialized(Guid.NewGuid(), clientId));

            return shoppingCart;
        }

        public void AddItem(Guid productId)
        {
            ApplyEvent(new ProductItemAddedToShoppingCart(Id, productId));
        }

        public void RemoveItem(Guid productId)
        {
            ApplyEvent(new ProductItemRemovedFromShoppingCart(Id, productId));
        }

        public void Confirm()
        {
            ApplyEvent(new ShoppingCartConfirmed(Id, DateTime.Now));
        }

        internal void Apply(ShoppingCartInitialized @event)
        {
            Id = @event.ShoppingCartId;
            ClientId = @event.ClientId;
            Status = ShoppingCartStatus.Pending;
            _products = new List<Guid>();
        }

        internal void Apply(ProductItemAddedToShoppingCart @event)
        {
            _products.Add(@event.ProductId);
        }

        internal void Apply(ProductItemRemovedFromShoppingCart @event)
        {
            _products.Remove(@event.ProductId);
        }

        internal void Apply(ShoppingCartConfirmed @event)
        {
            ConfirmedAt = @event.ConfirmedAt;
            Status = ShoppingCartStatus.Confirmed;
        }
    }
}
