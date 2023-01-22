using PluginAPI.Core;
using System;

namespace Mistaken.BetterSCP.SCP914.Events;

public class PlayerRecieveScp207EffectEventArgs : EventArgs
{
    internal PlayerRecieveScp207EffectEventArgs(Player player, byte intensity)
    {
        Player = player;
        Intensity = intensity;
    }

    public Player Player { get; set; }

    public byte Intensity { get; set; }
}
