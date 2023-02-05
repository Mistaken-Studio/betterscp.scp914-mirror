using System;
using System.Collections.Generic;
using System.Linq;
using Hazards;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Pickups;
using Mirror;
using Mistaken.API.Utilities;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using Scp914;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP914;

internal sealed class SCP914Handler
{
    public SCP914Handler()
    {
        EventManager.RegisterEvents(this);
    }

    ~SCP914Handler()
    {
        EventManager.UnregisterEvents(this);
    }

    private static readonly HashSet<DamageHandlerBase> _customDamageHandlers = new();
    private static GameObject _tantrumPrefab;
    private static Vector3 _outputPosition;
    private static Player _last914User;

    private static void SendAdminChat(string content)
    {
        foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
        {
            if ((referenceHub.serverRoles.AdminChatPerms || referenceHub.serverRoles.RaEverywhere) && referenceHub.Mode != ClientInstanceMode.Unverified)
                referenceHub.queryProcessor.TargetReply(referenceHub.queryProcessor.connectionToClient, content, true, false, string.Empty);
        }
    }

    private static bool OnRagdollSpawn(ReferenceHub player, DamageHandlerBase handler)
    {
        if (player is null)
            return false;

        if (_customDamageHandlers.Contains(handler))
            return false;

        return true;
    }

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    private void OnWaitingForPlayers()
    {
        _tantrumPrefab = NetworkClient.prefabs[Guid.Parse("a0e7ee93-b802-e5a4-38bd-95e27cc133ea")];
        _outputPosition = Scp914Controller.Singleton.OutputChamber.position + Vector3.up;
    }

    [PluginEvent(ServerEventType.RoundStart)]
    private void OnRoundStart()
        => _customDamageHandlers.Clear();

    [PluginEvent(ServerEventType.Scp914Activate)]
    private void OnScp914Activate(Player player, Scp914KnobSetting setting)
        => _last914User = player;

    [PluginEvent(ServerEventType.Scp914ProcessPlayer)]
    private void OnScp914ProcessPlayer(Player player, Scp914KnobSetting setting, Vector3 moveVector)
    {
        if (player.IsGodModeEnabled)
            return;

        if (player?.GameObject == null || !player.IsAlive)
            return;

        switch (setting)
        {
            case Scp914KnobSetting.Rough:
                {
                    var dmgHandler = new CustomReasonDamageHandler(Plugin.Translation.Scp914_Rough, Plugin.Instance.Config.DamageOnRough);
                    _customDamageHandlers.Add(dmgHandler);
                    var output = dmgHandler.ApplyDamage(player.ReferenceHub);
                    Events.Scp914.OnPlayerHurt(new Events.PlayerHurtEventArgs(dmgHandler, setting));

                    if (output == DamageHandlerBase.HandlerOutput.Death)
                    {
                        bool is0492 = player.Role == RoleTypeId.Scp0492;
                        player.ReferenceHub.playerStats.DealDamage(dmgHandler);
                        Events.Scp914.OnPlayerDied(new Events.PlayerDiedEventArgs(dmgHandler, setting));
                        SpawnPool();

                        if (player.UserId == _last914User.UserId && is0492)
                            SendAdminChat($"<color=orange>[<color=green>914</color>]</color> {player.Nickname} commited suicide in SCP-914!");
                    }

                    break;
                }

            case Scp914KnobSetting.Coarse:
                {
                    var dmgHandler = new CustomReasonDamageHandler(Plugin.Translation.Scp914_Coarse, Plugin.Instance.Config.DamageOnCoarse);
                    _customDamageHandlers.Add(dmgHandler);
                    var output = dmgHandler.ApplyDamage(player.ReferenceHub);
                    Events.Scp914.OnPlayerHurt(new Events.PlayerHurtEventArgs(dmgHandler, setting));

                    if (output == DamageHandlerBase.HandlerOutput.Death)
                    {
                        bool is0492 = player.Role == RoleTypeId.Scp0492;
                        player.ReferenceHub.playerStats.DealDamage(dmgHandler);
                        Events.Scp914.OnPlayerDied(new Events.PlayerDiedEventArgs(dmgHandler, setting));
                        SpawnPool();

                        if (player.UserId == _last914User.UserId && is0492)
                            SendAdminChat($"<color=orange>[<color=green>914</color>]</color> {player.Nickname} commited suicide in SCP-914!");
                    }

                    if (player.Team != Team.SCPs)
                        Item.CreatePickup(ItemType.Medkit, _outputPosition);

                    break;
                }

            case Scp914KnobSetting.VeryFine:
                {
                    if (player.Team == Team.SCPs)
                        return;

                    var num = UnityEngine.Random.Range(-10, 5);
                    byte min = player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Intensity;

                    if (num > 0)
                        Events.Scp914.OnPlayerRecieveScp207Effect(new Events.PlayerRecieveScp207EffectEventArgs(player, min));

                    player.ReferenceHub.playerEffectsController.ChangeState<CustomPlayerEffects.Scp207>((byte)Math.Max(num < 0 ? 0 : num, min));
                    break;
                }
        }
    }

    [PluginEvent(ServerEventType.Scp914UpgradePickup)]
    private void OnScp914UpgradePickup(ItemPickupBase pickup, Vector3 outputPosition, Scp914KnobSetting setting)
    {
        if (pickup.NetworkInfo.ItemId != ItemType.Coin)
            return;

        var random = UnityEngine.Random.Range(1, 101);
        switch (setting)
        {
            case Scp914KnobSetting.OneToOne:
                {
                    switch (random)
                    {
                        case int i when i <= 10:
                            {
                                Item.CreatePickup<AmmoPickup>(ItemType.Ammo12gauge, outputPosition + Vector3.up).NetworkSavedAmmo = 20;
                                break;
                            }

                        case int i when i <= 25:
                            {
                                Item.CreatePickup<AmmoPickup>(ItemType.Ammo44cal, outputPosition + Vector3.up).NetworkSavedAmmo = 16;
                                break;
                            }

                        case int i when i <= 40:
                            {
                                Item.CreatePickup<AmmoPickup>(ItemType.Ammo556x45, outputPosition + Vector3.up).NetworkSavedAmmo = 30;
                                break;
                            }

                        case int i when i <= 55:
                            {
                                Item.CreatePickup<AmmoPickup>(ItemType.Ammo762x39, outputPosition + Vector3.up).NetworkSavedAmmo = 30;
                                break;
                            }

                        case int i when i <= 70:
                            {
                                Item.CreatePickup<AmmoPickup>(ItemType.Ammo9x19, outputPosition + Vector3.up).NetworkSavedAmmo = 30;
                                break;
                            }
                    }

                    pickup.DestroySelf();
                    break;
                }

            case Scp914KnobSetting.Fine:
                {
                    switch (random)
                    {
                        case int i when i <= 6:
                            {
                                var firearm = Item.CreatePickup<FirearmPickup>(ItemType.GunCOM18, outputPosition + Vector3.up);
                                firearm.NetworkStatus = new FirearmStatus(9, firearm.Status.Flags, firearm.Status.Attachments);
                                break;
                            }

                        case int i when i <= 12:
                            {
                                var firearm = Item.CreatePickup<FirearmPickup>(ItemType.GunRevolver, outputPosition + Vector3.up);
                                firearm.NetworkStatus = new FirearmStatus(3, firearm.Status.Flags, firearm.Status.Attachments);
                                break;
                            }

                        case int i when i <= 25:
                            {
                                Item.CreatePickup(ItemType.ArmorLight, outputPosition + Vector3.up);
                                break;
                            }

                        case int i when i <= 50:
                            {
                                Item.CreatePickup(ItemType.Flashlight, outputPosition + Vector3.up);
                                break;
                            }
                    }

                    pickup.DestroySelf();
                    break;
                }

            case Scp914KnobSetting.VeryFine:
                {
                    switch (random)
                    {
                        case int i when i <= 1:
                            {
                                Item.CreatePickup(ItemType.KeycardFacilityManager, outputPosition + Vector3.up);
                                break;
                            }

                        case int i when i <= 10:
                            {
                                Item.CreatePickup(ItemType.KeycardGuard, outputPosition + Vector3.up);
                                break;
                            }

                        case int i when i <= 25:
                            {
                                Item.CreatePickup(ItemType.KeycardScientist, outputPosition + Vector3.up);
                                break;
                            }

                        case int i when i <= 50:
                            {
                                Item.CreatePickup(ItemType.KeycardJanitor, outputPosition + Vector3.up);
                                break;
                            }
                    }

                    pickup.DestroySelf();
                    break;
                }
        }
    }

    [PluginEvent(ServerEventType.PlayerDying)]
    private void OnPlayerDying(Player player, Player attacker, DamageHandlerBase handler)
    {
        if (player is null)
            return;

        if (_customDamageHandlers.Contains(handler))
            ServerDropEverything(player.ReferenceHub.inventory);
    }

    private void SpawnPool()
    {
        TantrumEnvironmentalHazard obj = UnityEngine.Object.Instantiate(_tantrumPrefab).GetComponent<TantrumEnvironmentalHazard>();
        obj.SynchronizedPosition = new RelativePosition(Scp914Controller.Singleton.OutputChamber.position);
        obj.transform.localScale = new Vector3(0.35f * obj.transform.localScale.x, obj.transform.localScale.y, 0.35f * obj.transform.localScale.z);
        NetworkServer.Spawn(obj.gameObject);
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
            var pickups = ServerDropAmmo(inv, item2, ushort.MaxValue);
            if (pickups is not null)
            {
                foreach (var pickup in pickups)
                {
                    pickup.transform.position = _outputPosition;
                    pickup.RefreshPositionAndRotation();
                }
            }
        }

        while (inv.UserInventory.Items.Count > 0)
        {
            var pickup = inv.ServerDropItem(inv.UserInventory.Items.ElementAt(0).Key);
            if (pickup is not null)
            {
                pickup.transform.position = _outputPosition;
                pickup.RefreshPositionAndRotation();
            }
        }

        HashSetPool<ItemType>.Shared.Return(hashSet);
    }

    private List<AmmoPickup> ServerDropAmmo(Inventory inv, ItemType ammoType, ushort amount, bool checkMinimals = false)
    {
        if (inv.UserInventory.ReserveAmmo.TryGetValue(ammoType, out ushort value) && InventoryItemLoader.AvailableItems.TryGetValue(ammoType, out ItemBase itemBase))
        {
            if (itemBase.PickupDropModel == null)
            {
                Debug.LogError("No pickup drop model set. Could not drop the ammo.");
                return null;
            }

            if (checkMinimals && itemBase.PickupDropModel is AmmoPickup ammoPickup)
            {
                int num = Mathf.FloorToInt(ammoPickup.SavedAmmo / 2f);
                if (amount < num && value > num)
                    amount = (ushort)num;
            }

            List<AmmoPickup> ammoPickups = new();
            int i = Mathf.Min(amount, value);
            inv.UserInventory.ReserveAmmo[ammoType] = (ushort)(value - i);
            inv.SendAmmoNextFrame = true;
            while (i > 0)
            {
                PickupSyncInfo psi = new(ammoType, inv.transform.position, Quaternion.identity, itemBase.Weight, 0);
                if (inv.ServerCreatePickup(itemBase, psi) is AmmoPickup ammoPickup2)
                {
                    ammoPickup2.NetworkSavedAmmo = (ushort)Mathf.Min(ammoPickup2.MaxAmmo, i);
                    i -= ammoPickup2.SavedAmmo;
                    ammoPickups.Add(ammoPickup2);
                }
                else
                    i--;
            }

            return ammoPickups;
        }

        return null;
    }
}
