// -----------------------------------------------------------------------
// <copyright file="Scp914PlayerDiedEventArgs.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Features;

namespace Mistaken.BetterSCP.SCP914.Events
{
    public class Scp914PlayerDiedEventArgs : EventArgs
    {
        public Scp914PlayerDiedEventArgs(Player target, Player killer, PlayerStats.HitInfo hitInfo, float damage, Scp914.Scp914KnobSetting knobSetting)
        {
            this.Target = target;
            this.Killer = killer;
            this.HitInfo = hitInfo;
            this.Damage = damage;
            this.KnobSetting = knobSetting;
        }

        public Player Target { get; set; }

        public Player Killer { get; set; }

        public PlayerStats.HitInfo HitInfo { get; set; }

        public float Damage { get; set; }

        public Scp914.Scp914KnobSetting KnobSetting { get; set; }
    }
}
