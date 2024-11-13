using System.Globalization;

namespace ServiceDefaults.Common;

public static class HandleTransforms
{
    public static TransformHandle ToLowercase(CultureInfo culture) =>
        handle => new Handle(handle.Components.Select(c => culture.TextInfo.ToLower(c)).ToArray());

    public static TransformHandle SplitOnWhiteSpace =>
        handle => new Handle(handle.Components
            .SelectMany(c => c.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
            .ToArray());

    public static TransformHandle IntoLetterAndDigitRuns =>
        handle => new Handle(handle.Components.SelectMany(SplitLetterAndDigitRuns).ToArray());

    private static IEnumerable<string> SplitLetterAndDigitRuns(string s)
    {
        int start = 0;
        while (start < s.Length && !char.IsLetterOrDigit(s[start]))
        {
            start += 1;
        }

        while (start < s.Length)
        {
            int end = start + 1;
            while (end < s.Length && char.IsLetterOrDigit(s[end]))
            {
                end += 1;
            }

            yield return s[start..end];
            start = end;
            while (start < s.Length && !char.IsLetterOrDigit(s[start]))
            {
                start += 1;
            }
        }
    }

    public static TransformHandle StopAtColon =>
        handle => new Handle(StopStringsAtColon(handle.Components).ToArray());

    private static IEnumerable<string> StopStringsAtColon(IEnumerable<string> strings)
    {
        foreach (string s in strings)
        {
            int colon = s.IndexOf(':');
            if (colon >= 0)
            {
                yield return s[..colon];
                yield break;
            }
            yield return s;
        }
    }
}
