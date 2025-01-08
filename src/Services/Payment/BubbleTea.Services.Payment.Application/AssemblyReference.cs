using System.Reflection;

namespace BubbleTea.Services.Payment.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
