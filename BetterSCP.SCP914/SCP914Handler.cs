// -----------------------------------------------------------------------
// <copyright file="SCP914Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Footprinting;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using NorthwoodLib.Pools;
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
            Mistaken.Events.Handlers.CustomEvents.SCP914Upgrading += this.CustomEvents_SCP914Upgrading;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Scp914.Activating -= this.Scp914_Activating;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= this.Scp914_UpgradingPlayer;
            Mistaken.Events.Handlers.CustomEvents.SCP914Upgrading -= this.CustomEvents_SCP914Upgrading;
        }

        private Player last914User;
        private Vector3 scp914OutputPosition = Vector3.zero;

        private void CustomEvents_SCP914Upgrading(Mistaken.Events.EventArgs.SCP914UpgradingEventArgs ev)
        {
            foreach (var player in RealPlayers.List.Where(p => p.IsReadyPlayer() && p.IsAlive && Vector3.Distance(p.Position, ev.OutputPosition) < 2))
                this.RunCoroutine(this.PunishOutput(player), "PunishOutput");
        }

        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            if (ev.IsAllowed)
                this.last914User = ev.Player;
        }

        private void Scp914_UpgradingPlayer(Exiled.Events.EventArgs.UpgradingPlayerEventArgs ev)
        {
            if (this.scp914OutputPosition != ev.OutputPosition)
                this.scp914OutputPosition = ev.OutputPosition;
            if (ev.Player.IsReadyPlayer() && ev.Player.IsAlive)
            {
                PlayerStats ps = ev.Player.ReferenceHub.playerStats;

                switch (ev.KnobSetting)
                {
                    case Scp914.Scp914KnobSetting.Rough:
                        {
                            var hitInfo = new PlayerStats.HitInfo(PluginHandler.Instance.Config.DamageOnRough, "*" + PluginHandler.Instance.Translation.Scp914_rough, DamageTypes.RagdollLess, this.last914User.Id, true);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(ev.Player, PluginHandler.Instance.Config.DamageOnRough, ev.KnobSetting));
                            var health = ps.Health + (ps.ArtificialNormalRatio * ps.ArtificialHealth);
                            if (health <= PluginHandler.Instance.Config.DamageOnRough)
                            {
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(ev.Player, this.last914User, hitInfo, PluginHandler.Instance.Config.DamageOnCoarse, ev.KnobSetting));
                                this.ServerDropEverything(ev.Player.Inventory);

                                bool is0492 = ev.Player.Role == RoleType.Scp0492;

                                ps.HurtPlayer(hitInfo, ev.Player.GameObject);

                                if (ev.Player.IsAlive)
                                    ev.Player.Kill(DamageTypes.RagdollLess);

                                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(PlayableScps.ScriptableObjects.ScpScriptableObjects.Instance.Scp173Data.TantrumPrefab);
                                gameObject.transform.position = Exiled.API.Features.Scp914.OutputBooth.position + (Exiled.API.Features.Scp914.OutputBooth.forward * 1f);
                                gameObject.transform.localScale = new Vector3(0.35f * gameObject.transform.localScale.x, gameObject.transform.localScale.y, 0.35f * gameObject.transform.localScale.z);
                                NetworkServer.Spawn(gameObject);

                                if (ev.Player.UserId == this.last914User.UserId && is0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }
                            else
                                ps.HurtPlayer(hitInfo, ev.Player.GameObject);
                        }

                        break;
                    case Scp914.Scp914KnobSetting.Coarse:
                        {
                            var hitInfo = new PlayerStats.HitInfo(PluginHandler.Instance.Config.DamageOnCoarse, "*" + PluginHandler.Instance.Translation.Scp914_coarse, DamageTypes.RagdollLess, this.last914User.Id, true);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(ev.Player, PluginHandler.Instance.Config.DamageOnCoarse, ev.KnobSetting));
                            var health = ps.Health + (ps.ArtificialNormalRatio * ps.ArtificialHealth);
                            if (health <= PluginHandler.Instance.Config.DamageOnCoarse)
                            {
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(ev.Player, this.last914User, hitInfo, PluginHandler.Instance.Config.DamageOnCoarse, ev.KnobSetting));
                                this.ServerDropEverything(ev.Player.Inventory);

                                bool is0492 = ev.Player.Role == RoleType.Scp0492;

                                ps.HurtPlayer(hitInfo, ev.Player.GameObject);

                                if (ev.Player.IsAlive)
                                    ev.Player.Kill(DamageTypes.RagdollLess);

                                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(PlayableScps.ScriptableObjects.ScpScriptableObjects.Instance.Scp173Data.TantrumPrefab);
                                gameObject.transform.position = Exiled.API.Features.Scp914.OutputBooth.position + (Exiled.API.Features.Scp914.OutputBooth.forward * 1f);
                                gameObject.transform.localScale = new Vector3(0.35f * gameObject.transform.localScale.x, gameObject.transform.localScale.y, 0.35f * gameObject.transform.localScale.z);
                                NetworkServer.Spawn(gameObject);

                                if (ev.Player.UserId == this.last914User.UserId && is0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }
                            else
                                ps.HurtPlayer(hitInfo, ev.Player.GameObject);
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
            }
        }

        private IEnumerator<float> PunishOutput(Player player)
        {
            var ps = player.ReferenceHub.playerStats;
            var health = ps.Health + (ps.ArtificialNormalRatio * ps.ArtificialHealth);

            for (int i = 0; i < 10 * 3; i++)
            {
                player.Hurt(player.Health / 5, DamageTypes.Bleeding, "*SCP 914");
                yield return Timing.WaitForSeconds(0.1f);
            }

            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(player, health - player.Health, isOutput: true));
        }

        private void ServerDropEverything(Inventory inv)
        {
            HashSet<ItemType> hashSet = HashSetPool<ItemType>.Shared.Rent();
            foreach (KeyValuePair<ItemType, ushort> item in inv.UserInventory.ReserveAmmo)
            {
                if (item.Value > 0)
                {
                    hashSet.Add(item.Key);
                }
            }

            foreach (ItemType item2 in hashSet)
            {
                var pickups = this.ServerDropAmmo(inv, item2, ushort.MaxValue);
                if (!(pickups is null))
                {
                    foreach (var pickup in pickups)
                    {
                        pickup.transform.position = this.scp914OutputPosition + Vector3.up;
                    }
                }
            }

            while (inv.UserInventory.Items.Count > 0)
            {
                var pickup = inv.ServerDropItem(inv.UserInventory.Items.ElementAt(0).Key);
                if (!(pickup is null))
                    pickup.transform.position = this.scp914OutputPosition + Vector3.up;
            }

            HashSetPool<ItemType>.Shared.Return(hashSet);
        }

        private List<AmmoPickup> ServerDropAmmo(Inventory inv, ItemType ammoType, ushort amount, bool checkMinimals = false)
        {
            if (inv.UserInventory.ReserveAmmo.TryGetValue(ammoType, out ushort value) && InventoryItemLoader.AvailableItems.TryGetValue(ammoType, out ItemBase value2))
            {
                if (value2.PickupDropModel == null)
                {
                    Debug.LogError("No pickup drop model set. Could not drop the ammo.");
                    return null;
                }

                AmmoPickup ammoPickup;
                if (checkMinimals && (object)(ammoPickup = value2.PickupDropModel as AmmoPickup) != null)
                {
                    int num = Mathf.FloorToInt((float)(int)ammoPickup.SavedAmmo / 2f);
                    if (amount < num && value > num)
                    {
                        amount = (ushort)num;
                    }
                }

                List<AmmoPickup> ammoPickups = new List<AmmoPickup>();
                int num2 = Mathf.Min(amount, value);
                inv.UserInventory.ReserveAmmo[ammoType] = (ushort)(value - num2);
                inv.SendAmmoNextFrame = true;
                while (num2 > 0)
                {
                    PickupSyncInfo pickupSyncInfo = default(PickupSyncInfo);
                    pickupSyncInfo.ItemId = ammoType;
                    pickupSyncInfo.Serial = ItemSerialGenerator.GenerateNext();
                    pickupSyncInfo.Weight = value2.Weight;
                    pickupSyncInfo.Position = inv.transform.position;
                    PickupSyncInfo psi = pickupSyncInfo;
                    AmmoPickup ammoPickup2;
                    if ((object)(ammoPickup2 = inv.ServerCreatePickup(value2, psi) as AmmoPickup) != null)
                    {
                        ushort num4 = ammoPickup2.NetworkSavedAmmo = (ushort)Mathf.Min(ammoPickup2.MaxAmmo, num2);
                        num2 -= ammoPickup2.SavedAmmo;
                        ammoPickups.Add(ammoPickup2);
                    }
                    else
                    {
                        num2--;
                    }
                }

                return ammoPickups;
            }

            return null;
        }
    }
}
