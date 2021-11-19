// -----------------------------------------------------------------------
// <copyright file="EventHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.Events.Extensions;
using static Exiled.Events.Events;

namespace Mistaken.BetterSCP.SCP914.Events
{
    public static class EventHandler
    {
        public static event CustomEventHandler<Scp914PlayerDiedEventArgs> Scp914PlayerDied;

        public static event CustomEventHandler<Scp914PlayerHurtEventArgs> Scp914PlayerHurt;

        public static event CustomEventHandler<Scp914PlayerRecieveScp207EffectEventArgs> Scp914PlayerRecieveScp207Effect;

        internal static void OnScp914PlayerDied(Scp914PlayerDiedEventArgs ev)
        {
            Scp914PlayerDied.InvokeSafely(ev);
        }

        internal static void OnScp914PlayerHurt(Scp914PlayerHurtEventArgs ev)
        {
            Scp914PlayerHurt.InvokeSafely(ev);
        }

        internal static void OnScp914PlayerRecieveScp207Effect(Scp914PlayerRecieveScp207EffectEventArgs ev)
        {
            Scp914PlayerRecieveScp207Effect.InvokeSafely(ev);
        }
    }
}
