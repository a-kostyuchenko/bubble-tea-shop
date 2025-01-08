using System.Reflection;

namespace BubbleTea.Services.Payment.Infrastructure;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
