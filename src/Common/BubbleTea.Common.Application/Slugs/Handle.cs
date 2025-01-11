namespace BubbleTea.Common.Application.Slugs;

public sealed record Handle(params string[] Components);

public delegate Slug HandleToSlug(Handle handle);

public delegate Handle TransformHandle(Handle handle);

public static class HandleTransformCompositions
{
    public static TransformHandle Then(this TransformHandle first, TransformHandle second) =>
        handle => second(first(handle));

    public static Handle Transform(this Handle handle, params TransformHandle[] transforms) =>
        transforms.Aggregate(handle, (current, transform) => transform(current));

    public static Slug ToSlug(this Handle handle, HandleToSlug conversion) =>
        conversion(handle);
}
