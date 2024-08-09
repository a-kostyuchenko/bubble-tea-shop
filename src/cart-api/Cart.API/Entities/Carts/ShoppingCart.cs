using Cart.API.Entities.Carts.Events;
using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class ShoppingCart : Entity
{
    private ShoppingCart() : base(Ulid.NewUlid())
    {
        Status = CartStatus.Draft;
    }
    
    private readonly List<CartItem> _items = [];
    
    public string Customer { get; private set; }
    public string? Note { get; private set; }
    public CartStatus Status { get; private set; }
    public IReadOnlyCollection<CartItem> Items => [.. _items];
    
    public static Result<ShoppingCart> Create(string customer)
    {
        if (string.IsNullOrWhiteSpace(customer))
        {
            return Result.Failure<ShoppingCart>(CartErrors.CustomerIsMissing);
        }
        
        return new ShoppingCart
        {
            Customer = customer
        };
    }
    
    public void AddItem(CartItem item)
    {
        _items.Add(item);
    }
    
    public Result RemoveItem(Guid itemId)
    {
        CartItem? item = _items.Find(x => x.Id == itemId);
        
        if (item is null)
        {
            return Result.Failure(CartItemErrors.NotFound(itemId));
        }
        
        _items.Remove(item);
        return Result.Success();
    } 
    
    public Result CheckOut()
    {
        if (_items.Count == 0)
        {
            return Result.Failure(CartErrors.EmptyCart);
        }

        if (Status == CartStatus.CheckedOut)
        {
            return Result.Failure(CartErrors.AlreadyCheckedOut);
        }

        if (Status == CartStatus.Cancelled)
        {
            return Result.Failure(CartErrors.Cancelled);
        }
        
        Status = CartStatus.CheckedOut;
        
        Raise(new CartCheckedOutDomainEvent(Id));

        return Result.Success();
    }
    
    public Result Cancel()
    {
        if (Status == CartStatus.CheckedOut)
        {
            return Result.Failure(CartErrors.AlreadyCheckedOut);
        }

        if (Status == CartStatus.Cancelled)
        {
            return Result.Failure(CartErrors.Cancelled);
        }
        
        Status = CartStatus.Cancelled;
        
        return Result.Success();
    }
}
