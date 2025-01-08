namespace BubbleTea.ServiceDefaults.Common;

public static class HandleToSlugConversions
{
    public static HandleToSlug Concatenate =>
        handle => new Slug(string.Join(string.Empty, handle.Components));

    public static HandleToSlug Hyphenate =>
        handle => new Slug(string.Join('-', handle.Components));
}
