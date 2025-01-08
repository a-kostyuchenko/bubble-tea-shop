using System.Reflection;

namespace BubbleTea.Services.Orders.API;

internal static class AssemblyReference
{
    internal static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
