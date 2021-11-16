// -----------------------------------------------------------------------
// <copyright file="SCP914Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using MEC;
using Mirror;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP914
{
    internal class SCP914Handler : Module
    {
        public SCP914Handler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
        }

        public override string Name => nameof(SCP914Handler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Scp914.Activating += this.Scp914_Activating;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += this.Scp914_UpgradingPlayer;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Scp914.Activating -= this.Scp914_Activating;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= this.Scp914_UpgradingPlayer;
        }

        private Player last914User;

        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            if (ev.IsAllowed)
                this.last914User = ev.Player;
        }

        private void Scp914_UpgradingPlayer(Exiled.Events.EventArgs.UpgradingPlayerEventArgs ev)
        {
            if (ev.Player.IsReadyPlayer() && ev.Player.IsAlive)
            {
                PlayerStats ps = ev.Player.ReferenceHub.playerStats;

                switch (ev.KnobSetting)
                {
                    case Scp914.Scp914KnobSetting.Rough:
                        {
                            int damage = 500;
                            var hitInfo = new PlayerStats.HitInfo(damage, "*" + PluginHandler.Instance.Translation.Scp914_rough, new DamageTypes.DamageType("914"), this.last914User.Id, false);
                            if (ev.Player.Health <= damage)
                            {
                                // CustomAchievements.RoundEventHandler.AddProggress("914Killer", player);
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(ev.Player, this.last914User, hitInfo, damage, ev.KnobSetting));
                                if (ev.Player.UserId == this.last914User.UserId && ev.Player.Role == RoleType.Scp0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }

                            ps.HurtPlayer(hitInfo, ev.Player.GameObject);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(ev.Player, damage, ev.KnobSetting));
                        }

                        break;
                    case Scp914.Scp914KnobSetting.Coarse:
                        {
                            int damage = 250;
                            var hitInfo = new PlayerStats.HitInfo(damage, "*" + PluginHandler.Instance.Translation.Scp914_coarse, new DamageTypes.DamageType("914"), this.last914User.Id, false);
                            if (ev.Player.Health <= damage)
                            {
                                // CustomAchievements.RoundEventHandler.AddProggress("914Killer", player);
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(ev.Player, this.last914User, hitInfo, damage, ev.KnobSetting));
                                if (ev.Player.UserId == this.last914User.UserId && ev.Player.Role == RoleType.Scp0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }

                            ps.HurtPlayer(hitInfo, ev.Player.GameObject);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(ev.Player, damage, ev.KnobSetting));
                            if (ev.Player.Team != Team.SCP)
                                new Usable(ItemType.Medkit).Spawn(ev.OutputPosition + Vector3.up);
                        }

                        break;
                    case Scp914.Scp914KnobSetting.VeryFine:
                        {
                            if (ev.Player.Team == Team.SCP)
                                return;
                            var num = UnityEngine.Random.Range(-10, 5);
                            byte min = ev.Player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Intensity;
                            if (num > 0)
                                Events.EventHandler.OnScp914PlayerRecieveScp207Effect(new Events.Scp914PlayerRecieveScp207EffectEventArgs(ev.Player, min));
                            ev.Player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<CustomPlayerEffects.Scp207>((byte)Mathf.Max(num < 0 ? 0 : num, min));
                        }

                        break;
                }

                if (Vector3.Distance(ev.Player.Position, ev.OutputPosition) < 2)
                    this.RunCoroutine(this.PunishOutput(ev.Player), "PunishOutput");
            }
        }

        private IEnumerator<float> PunishOutput(Player player)
        {
            var maxPlayerHealth = player.Health;
            for (int i = 0; i < 10 * 3; i++)
            {
                player.Hurt(player.Health / 50, new DamageTypes.DamageType("SCP 914"), "*SCP 914");
                yield return Timing.WaitForSeconds(0.1f);
            }

            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(player, maxPlayerHealth - player.Health, isOutput: true));
        }
    }
}
