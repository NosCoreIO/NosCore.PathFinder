using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Enable System.Drawing.Common Unix support before any static initializers run
        // Try multiple switch names for compatibility
        System.AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
        System.Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_ENABLEUNIXSUPPORT", "1");
    }
}
