using System.Reflection;

namespace BubbleTea.Services.Payment.API;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
