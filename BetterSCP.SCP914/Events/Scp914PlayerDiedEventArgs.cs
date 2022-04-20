// -----------------------------------------------------------------------
// <copyright file="Scp914PlayerDiedEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Features.DamageHandlers;

namespace Mistaken.BetterSCP.SCP914.Events
{
    public class Scp914PlayerDiedEventArgs : EventArgs
    {
        public Scp914PlayerDiedEventArgs(DamageHandler handler, Scp914.Scp914KnobSetting knobSetting)
        {
            this.Handler = handler;
            this.KnobSetting = knobSetting;
        }

        public DamageHandler Handler { get; set; }

        public Scp914.Scp914KnobSetting KnobSetting { get; set; }
    }
}
