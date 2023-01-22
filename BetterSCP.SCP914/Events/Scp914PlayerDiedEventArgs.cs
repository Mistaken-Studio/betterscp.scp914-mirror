using PlayerStatsSystem;
using Scp914;
using System;

namespace Mistaken.BetterSCP.SCP914.Events;

public class PlayerDiedEventArgs : EventArgs
{
    internal PlayerDiedEventArgs(DamageHandlerBase handler, Scp914KnobSetting knobSetting)
    {
        Handler = handler;
        KnobSetting = knobSetting;
    }

    public DamageHandlerBase Handler { get; set; }

    public Scp914KnobSetting KnobSetting { get; set; }
}
