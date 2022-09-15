// -----------------------------------------------------------------------
// <copyright file="SCP914Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
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
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Scp914.Activating += this.Scp914_Activating;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += this.Scp914_UpgradingPlayer;
            Exiled.Events.Handlers.Scp914.UpgradingItem += this.Scp914_UpgradingItem;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.SpawningRagdoll += this.Player_SpawningRagdoll;
            Mistaken.Events.Handlers.CustomEvents.SCP914Upgrading += this.CustomEvents_SCP914Upgrading;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Scp914.Activating -= this.Scp914_Activating;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= this.Scp914_UpgradingPlayer;
            Exiled.Events.Handlers.Scp914.UpgradingItem -= this.Scp914_UpgradingItem;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.SpawningRagdoll -= this.Player_SpawningRagdoll;
            Mistaken.Events.Handlers.CustomEvents.SCP914Upgrading -= this.CustomEvents_SCP914Upgrading;
        }

        private Player last914User;
        private Vector3 scp914OutputPosition = Vector3.zero;
        private HashSet<PlayerStatsSystem.DamageHandlerBase> customDamageHandlers = new HashSet<PlayerStatsSystem.DamageHandlerBase>();

        private void CustomEvents_SCP914Upgrading(Mistaken.Events.EventArgs.SCP914UpgradingEventArgs ev)
        {
            foreach (var player in RealPlayers.List.Where(p => p.IsReadyPlayer() && p.IsAlive && Vector3.Distance(p.Position, ev.OutputPosition) < 2))
                this.RunCoroutine(this.PunishOutput(player), "PunishOutput");
        }

        private void Player_SpawningRagdoll(Exiled.Events.EventArgs.SpawningRagdollEventArgs ev)
        {
            if (!this.customDamageHandlers.Contains(ev.DamageHandlerBase))
                return;
            ev.IsAllowed = false;
        }

        private void Server_RoundStarted()
        {
            this.customDamageHandlers.Clear();
        }

        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            if (ev.IsAllowed)
                this.last914User = ev.Player;
        }

        private void Scp914_UpgradingPlayer(Exiled.Events.EventArgs.UpgradingPlayerEventArgs ev)
        {
            if (ev.Player.IsGodModeEnabled)
                return;
            if (this.scp914OutputPosition != ev.OutputPosition)
                this.scp914OutputPosition = ev.OutputPosition;
            if (ev.Player.IsReadyPlayer() && ev.Player.IsAlive)
            {
                switch (ev.KnobSetting)
                {
                    case Scp914.Scp914KnobSetting.Rough:
                        {
                            var dmgHandler = new DamageHandler(ev.Player, new CustomReasonDamageHandler("Melted using SCP914", PluginHandler.Instance.Config.DamageOnRough));
                            dmgHandler.Attacker = this.last914User;
                            this.customDamageHandlers.Add(dmgHandler.Base);
                            var reason = dmgHandler.Base.ApplyDamage(ev.Player.ReferenceHub);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(dmgHandler, ev.KnobSetting));
                            if (reason == PlayerStatsSystem.DamageHandlerBase.HandlerOutput.Death)
                            {
                                bool is0492 = ev.Player.Role == RoleType.Scp0492;
                                ev.Player.Hurt(dmgHandler.Base);
                                dmgHandler = new DamageHandler(ev.Player, new CustomReasonDamageHandler(PluginHandler.Instance.Translation.Scp914_rough, PluginHandler.Instance.Config.DamageOnCoarse));
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(dmgHandler, ev.KnobSetting));

                                this.SpawnPool();

                                if (ev.Player.UserId == this.last914User.UserId && is0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }
                        }

                        break;
                    case Scp914.Scp914KnobSetting.Coarse:
                        {
                            var dmgHandler = new DamageHandler(ev.Player, new CustomReasonDamageHandler("Melted using SCP914", PluginHandler.Instance.Config.DamageOnCoarse));
                            dmgHandler.Attacker = this.last914User;
                            this.customDamageHandlers.Add(dmgHandler.Base);
                            var reason = dmgHandler.Base.ApplyDamage(ev.Player.ReferenceHub);
                            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(dmgHandler, ev.KnobSetting));
                            if (reason == PlayerStatsSystem.DamageHandlerBase.HandlerOutput.Death)
                            {
                                bool is0492 = ev.Player.Role == RoleType.Scp0492;
                                dmgHandler = new DamageHandler(ev.Player, new CustomReasonDamageHandler(PluginHandler.Instance.Translation.Scp914_coarse, PluginHandler.Instance.Config.DamageOnCoarse));
                                ev.Player.Hurt(dmgHandler.Base);
                                Events.EventHandler.OnScp914PlayerDied(new Events.Scp914PlayerDiedEventArgs(dmgHandler, ev.KnobSetting));

                                this.SpawnPool();

                                if (ev.Player.UserId == this.last914User.UserId && is0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{this.last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }

                            if (ev.Player.Role.Team != Team.SCP)
                                Item.Create(ItemType.Medkit).Spawn(ev.OutputPosition + Vector3.up);
                        }

                        break;
                    case Scp914.Scp914KnobSetting.VeryFine:
                        {
                            if (ev.Player.Role.Team == Team.SCP)
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

        private void Scp914_UpgradingItem(Exiled.Events.EventArgs.UpgradingItemEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Item.Type != ItemType.Coin)
                return;

            var random = UnityEngine.Random.Range(1, 101);
            switch (ev.KnobSetting)
            {
                case Scp914.Scp914KnobSetting.OneToOne:
                    {
                        switch (random)
                        {
                            case int i when i <= 10:
                                {
                                    (Item.Create(ItemType.Ammo12gauge).Spawn(ev.OutputPosition + Vector3.up).Base as AmmoPickup).NetworkSavedAmmo = 20;
                                    break;
                                }

                            case int i when i <= 25:
                                {
                                    (Item.Create(ItemType.Ammo44cal).Spawn(ev.OutputPosition + Vector3.up).Base as AmmoPickup).NetworkSavedAmmo = 16;
                                    break;
                                }

                            case int i when i <= 40:
                                {
                                    (Item.Create(ItemType.Ammo556x45).Spawn(ev.OutputPosition + Vector3.up).Base as AmmoPickup).NetworkSavedAmmo = 30;
                                    break;
                                }

                            case int i when i <= 55:
                                {
                                    (Item.Create(ItemType.Ammo762x39).Spawn(ev.OutputPosition + Vector3.up).Base as AmmoPickup).NetworkSavedAmmo = 30;
                                    break;
                                }

                            case int i when i <= 70:
                                {
                                    (Item.Create(ItemType.Ammo9x19).Spawn(ev.OutputPosition + Vector3.up).Base as AmmoPickup).NetworkSavedAmmo = 30;
                                    break;
                                }

                            default:
                                break;
                        }

                        ev.Item.Destroy();
                    }

                    break;

                case Scp914.Scp914KnobSetting.Fine:
                    {
                        switch (random)
                        {
                            case int i when i <= 6:
                                {
                                    var firearm = Item.Create(ItemType.GunCOM18).Spawn(ev.OutputPosition + Vector3.up).Base as FirearmPickup;
                                    firearm.NetworkStatus = new FirearmStatus(2, firearm.NetworkStatus.Flags, firearm.NetworkStatus.Attachments);
                                    break;
                                }

                            case int i when i <= 12:
                                {
                                    var firearm = Item.Create(ItemType.GunRevolver).Spawn(ev.OutputPosition + Vector3.up).Base as FirearmPickup;
                                    firearm.NetworkStatus = new FirearmStatus(2, firearm.NetworkStatus.Flags, firearm.NetworkStatus.Attachments);
                                    break;
                                }

                            case int i when i <= 25:
                                {
                                    Item.Create(ItemType.ArmorLight).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            case int i when i <= 50:
                                {
                                    Item.Create(ItemType.Flashlight).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            default:
                                break;
                        }

                        ev.Item.Destroy();
                    }

                    break;

                case Scp914.Scp914KnobSetting.VeryFine:
                    {
                        switch (random)
                        {
                            case int i when i <= 1:
                                {
                                    Item.Create(ItemType.KeycardFacilityManager).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            case int i when i <= 10:
                                {
                                    Item.Create(ItemType.KeycardGuard).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            case int i when i <= 25:
                                {
                                    Item.Create(ItemType.KeycardScientist).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            case int i when i <= 50:
                                {
                                    Item.Create(ItemType.KeycardJanitor).Spawn(ev.OutputPosition + Vector3.up);
                                    break;
                                }

                            default:
                                break;
                        }

                        ev.Item.Destroy();
                    }

                    break;
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (this.customDamageHandlers.Contains(ev.Handler.Base))
                this.ServerDropEverything(ev.Target.Inventory);
        }

        private IEnumerator<float> PunishOutput(Player player)
        {
            float damage = 0f;
            for (int i = 0; i < 10 * 3; i++)
            {
                if (!player.IsAlive)
                    break;
                damage += player.Health / 5;
                player.Hurt(new CustomReasonDamageHandler("Stealing is bad :/", player.Health / 5));
                yield return Timing.WaitForSeconds(0.1f);
            }

            var dmgHandler = new DamageHandler(player, new CustomReasonDamageHandler("Stealing is bad :/", damage));
            dmgHandler.Attacker = this.last914User;
            Events.EventHandler.OnScp914PlayerHurt(new Events.Scp914PlayerHurtEventArgs(dmgHandler, isOutput: true));
        }

        private void ServerDropEverything(Inventory inv)
        {
            HashSet<ItemType> hashSet = HashSetPool<ItemType>.Shared.Rent();
            foreach (KeyValuePair<ItemType, ushort> item in inv.UserInventory.ReserveAmmo)
            {
                if (item.Value > 0)
                    hashSet.Add(item.Key);
            }

            foreach (ItemType item2 in hashSet)
            {
                var pickups = this.ServerDropAmmo(inv, item2, ushort.MaxValue);
                if (!(pickups is null))
                {
                    foreach (var pickup in pickups)
                        pickup.transform.position = this.scp914OutputPosition + Vector3.up;
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

        private void SpawnPool()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(PlayableScps.ScriptableObjects.ScpScriptableObjects.Instance.Scp173Data.TantrumPrefab);
            gameObject.transform.position = Exiled.API.Features.Scp914.OutputBooth.position + (Exiled.API.Features.Scp914.OutputBooth.forward * 1f);
            gameObject.transform.localScale = new Vector3(0.35f * gameObject.transform.localScale.x, gameObject.transform.localScale.y, 0.35f * gameObject.transform.localScale.z);
            NetworkServer.Spawn(gameObject);
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
                if (checkMinimals && (ammoPickup = value2.PickupDropModel as AmmoPickup) != null)
                {
                    int num = Mathf.FloorToInt(ammoPickup.SavedAmmo / 2f);
                    if (amount < num && value > num)
                        amount = (ushort)num;
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
                    if ((ammoPickup2 = inv.ServerCreatePickup(value2, psi) as AmmoPickup) != null)
                    {
                        ammoPickup2.NetworkSavedAmmo = (ushort)Mathf.Min(ammoPickup2.MaxAmmo, num2);
                        num2 -= ammoPickup2.SavedAmmo;
                        ammoPickups.Add(ammoPickup2);
                    }
                    else
                        num2--;
                }

                return ammoPickups;
            }

            return null;
        }
    }
}
