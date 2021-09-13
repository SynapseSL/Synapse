using CommandSystem.Commands.Shared;
using Synapse.Api;

public static class SynapseVersion
{
    public const int Major = 2;

    public const int Minor = 7;

    public const int Patch = 0;

    public const VersionType Type = VersionType.Pre;

    public const string SubVersion = "1.5";

    public const string BasedGameVersion = "11.0.0-parabellum-public-2.5-9dee09b1";

    public static bool Debug { get; private set; } = false;

    public static string GetVersionName()
    {
        var version = $"{Major}.{Minor}.{Patch}";

        if (Type != VersionType.None)
            version += $"-{Type}-{SubVersion}";

        if (Debug)
            version += "-DEBUG";

        return version;
    }

    internal static void Init()
    {
#if DEBUG
        Debug = true;
#endif
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = $"Plugin Framework: Synapse\nSynapse Version: {GetVersionName()}\nDescription: Synapse is a heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";

        if (Debug)
            Logger.Get.Warn("Debug Version of Synapse loaded! This Version should only be used for testing and not playing");

        if (BasedGameVersion != GameCore.Version.VersionString)
            Logger.Get.Warn("Synapse-Version: Different Game Version than expected. Bugs may occurre");
    }

    public enum VersionType
    {
        None,
        Pre,
        Dev
    }
}
