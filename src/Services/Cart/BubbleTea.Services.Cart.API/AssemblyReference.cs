using System.Reflection;

namespace BubbleTea.Services.Cart.API;

internal static class AssemblyReference
{
    internal static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
