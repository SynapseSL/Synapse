using CommandSystem.Commands.Shared;
using Synapse.Api;

public static class SynapseVersion
{
    public const int Major = 2;

    public const int Minor = 9;

    public const int Patch = 0;

    public const VersionType Type = VersionType.None;

    public const string SubVersion = "1.0";

    public const string BasedGameVersion = "11.1.3";

    public static bool Debug { get; private set; } = false;

    public static string GetVersionName()
    {
        var version = $"{Major}.{Minor}.{Patch}";

        if (Type != VersionType.None)
            version += $" {Type} {SubVersion}";

        if (Debug)
            version += " DEBUG";

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
            Logger.Get.Warn("A Debug Build of Synapse was loaded! This version should only be used for testing and not playing as it loads for longer and is less stable.");

        if (BasedGameVersion != GameCore.Version.VersionString)
            Logger.Get.Warn("Synapse-Version: Different game version than expected. Bugs may occur!");
    }

    public enum VersionType
    {
        None,
        Pre,
        Dev
    }
}
