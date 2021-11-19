// -----------------------------------------------------------------------
// <copyright file="Scp914PlayerRecieveScp207EffectEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Features;

namespace Mistaken.BetterSCP.SCP914.Events
{
    public class Scp914PlayerRecieveScp207EffectEventArgs : EventArgs
    {
        public Scp914PlayerRecieveScp207EffectEventArgs(Player player, byte intensity)
        {
            this.Player = player;
            this.Intensity = intensity;
        }

        public Player Player { get; set; }

        public byte Intensity { get; set; }
    }
}
