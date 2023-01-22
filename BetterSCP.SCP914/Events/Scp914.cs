using System;

namespace Mistaken.BetterSCP.SCP914.Events;

public static class Scp914
{
    public static event Action<PlayerDiedEventArgs> PlayerDied;

    public static event Action<PlayerHurtEventArgs> PlayerHurt;

    public static event Action<PlayerRecieveScp207EffectEventArgs> PlayerRecieveScp207Effect;

    internal static void OnPlayerDied(PlayerDiedEventArgs ev) => PlayerDied?.Invoke(ev);

    internal static void OnPlayerHurt(PlayerHurtEventArgs ev) => PlayerHurt?.Invoke(ev);

    internal static void OnPlayerRecieveScp207Effect(PlayerRecieveScp207EffectEventArgs ev) => PlayerRecieveScp207Effect?.Invoke(ev);
}
