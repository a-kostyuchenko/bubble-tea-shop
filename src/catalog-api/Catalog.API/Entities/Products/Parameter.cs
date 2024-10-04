using ServiceDefaults.Domain;

namespace Catalog.API.Entities.Products;

public sealed class Parameter : Entity
{
    private Parameter() : base(Ulid.NewUlid())
    {
    }

    private readonly HashSet<Option> _options = [];

    public string Name { get; private set; }
    public IReadOnlyCollection<Option> Options => [.. _options];
    
    public static Parameter Create(string name) => new() { Name = name };
    
    public void AddOption(string name, double value, Money extraPrice) => 
        _options.Add(Option.Create(name, value, extraPrice));
}
