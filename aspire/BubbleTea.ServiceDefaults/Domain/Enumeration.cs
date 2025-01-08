using System.Reflection;

namespace BubbleTea.ServiceDefaults.Domain;
public abstract class Enumeration<TEnum>() : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{

    private static readonly Lazy<Dictionary<int, TEnum>> EnumerationsDictionary =
        new(() => CreateEnumerationDictionary(typeof(TEnum)));

    protected Enumeration(int id, string name)
        : this()
    {
        Id = id;
        Name = name;
    }

    public int Id { get; protected init; }
    public string Name { get; protected init; }

    public static bool operator ==(Enumeration<TEnum>? a, Enumeration<TEnum>? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Enumeration<TEnum>? first, Enumeration<TEnum>? second) =>
        !(first == second);

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null)
        {
            return false;
        }

        return other.GetType() == GetType() && other.Id.Equals(Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return obj is Enumeration<TEnum> other && other.Id.Equals(Id);
    }

    public override int GetHashCode() =>
        Id.GetHashCode() * 37;

    private static Dictionary<int, TEnum> CreateEnumerationDictionary(Type enumType) =>
        GetFieldsForType(enumType).ToDictionary(t => t.Id);

    private static IEnumerable<TEnum> GetFieldsForType(Type enumType) =>
        enumType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fieldInfo => enumType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

    public static IReadOnlyCollection<TEnum> GetValues() => [.. EnumerationsDictionary.Value.Values];

    public static TEnum? FromId(int id) =>
        EnumerationsDictionary.Value.GetValueOrDefault(id);

    public static TEnum? FromName(string name) =>
        EnumerationsDictionary.Value.Values.SingleOrDefault(x => x.Name == name);

    public static bool Contains(int id) => EnumerationsDictionary.Value.ContainsKey(id);
}
