using System.Runtime.InteropServices;

namespace N3O.Tool.Utilities;

public static class Host {
    public static bool IsPosix => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                                  RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) || 
                                  RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}

