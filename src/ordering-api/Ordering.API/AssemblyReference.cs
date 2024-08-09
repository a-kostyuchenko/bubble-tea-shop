using System.Reflection;

namespace Ordering.API;

internal static class AssemblyReference
{
    internal static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
