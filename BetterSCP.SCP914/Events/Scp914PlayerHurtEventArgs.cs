using PlayerStatsSystem;
using Scp914;
using System;

namespace Mistaken.BetterSCP.SCP914.Events;

public class PlayerHurtEventArgs : EventArgs
{
    internal PlayerHurtEventArgs(DamageHandlerBase handler, Scp914KnobSetting knobSetting = default, bool isOutput = false)
    {
        Handler = handler;
        KnobSetting = knobSetting;
        IsOutput = isOutput;
    }

    public DamageHandlerBase Handler { get; set; }

    public Scp914KnobSetting KnobSetting { get; set; }

    public bool IsOutput { get; set; }
}
