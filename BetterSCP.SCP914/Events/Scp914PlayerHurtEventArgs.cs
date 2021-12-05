// -----------------------------------------------------------------------
// <copyright file="Scp914PlayerHurtEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Features;

namespace Mistaken.BetterSCP.SCP914.Events
{
    public class Scp914PlayerHurtEventArgs : EventArgs
    {
        public Scp914PlayerHurtEventArgs(DamageHandler handler, Scp914.Scp914KnobSetting knobSetting = default, bool isOutput = false)
        {
            this.Handler = handler;
            this.KnobSetting = knobSetting;
            this.IsOutput = isOutput;
        }

        public DamageHandler Handler { get; set; }

        public bool IsOutput { get; set; }

        public Scp914.Scp914KnobSetting KnobSetting { get; set; }
    }
}
