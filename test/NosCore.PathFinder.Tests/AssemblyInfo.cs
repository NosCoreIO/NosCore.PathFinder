using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Enable System.Drawing.Common Unix support before any static initializers run
        System.AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
    }
}
