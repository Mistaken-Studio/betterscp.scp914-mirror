using HarmonyLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace Mistaken.BetterSCP.SCP914;

internal sealed class Plugin
{
    public static Plugin Instance { get; private set; }

    public static Translation Translation { get; } = new Translation();

    [PluginConfig]
    public Config Config;

    private static readonly Harmony _harmony = new("mistaken.scp914.com");

    [PluginPriority(LoadPriority.Medium)]
    [PluginEntryPoint("BetterSCP-SCP914", "1.0.0", "", "Mistaken Devs")]
    private void Load()
    {
        Instance = this;
        _harmony.PatchAll();
        new SCP914Handler();
    }

    [PluginUnload]
    private void Unload()
    {
        _harmony.UnpatchAll();
    }
}
