using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;
using CheckpointDoor = LabApi.Features.Wrappers.CheckpointDoor;
using ElevatorDoor = LabApi.Features.Wrappers.ElevatorDoor;

using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MapGeneration;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace AntiSuicidalZombies
{
    public class MainClass : Plugin<Config>
    {
        public override void Enable()
        {
            PlayerEvents.Hurting += this.OnPlayerHurting;
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= this.OnPlayerHurting;
        }

        internal void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Player.Role != RoleTypeId.Scp0492 || ev.DamageHandler is not UniversalDamageHandler udh)
            {
                return;
            }
            if (udh.TranslationId == DeathTranslations.Tesla.Id)
            {
                this.ApplyEffects(ev.Player, Config.TeslaEffects);  
                ev.IsAllowed = false;
                return;
            }
            if (udh.TranslationId == DeathTranslations.Falldown.Id && fallingZombies.Remove(ev.Player))
            {
                ev.IsAllowed = false;
                return;
            }
            if (udh.TranslationId == DeathTranslations.Crushed.Id)
            {
                if (udh.Damage == 200f)
                {
                    this.ApplyEffects(ev.Player, Config.CrushedEffects);
                    ev.IsAllowed = false;
                    return;
                }
                Vector3 position;
                if (ev.Player.Room.Name is RoomName.HczWarhead or RoomName.HczServers)
                {
                    Vector3 nukeElevator = ElevatorDoor.List.Select(e => e.Base.TargetPosition).Aggregate((e1, e2) =>
                                           Vector3.Distance(ev.Player.Position, e1) < Vector3.Distance(ev.Player.Position, e2) ? e1 : e2);
                    Vector3 nukePanel = AlphaWarheadNukesitePanel.Singleton.transform.position;
                    if (Vector3.Distance(ev.Player.Position, nukeElevator) < Vector3.Distance(ev.Player.Position, nukePanel))
                    {
                        position = nukeElevator + 3 * Vector3.up;
                        this.ApplyEffects(ev.Player, Config.CrushedEffects);
                    }
                    else
                    {
                        position = nukePanel + 3 * Vector3.up;
                        fallingZombies.Add(ev.Player);
                    }
                }
                else
                {
                    IEnumerable<Door> doors = ev.Player.Room.Doors.Where(d => d.Permissions == DoorPermissionFlags.None && !CheckpointDoor.Dictionary.Keys.SelectMany(c => c.SubDoors).Contains(d.Base));//!CheckpointDoor.List.SelectMany(c => c.SubDoors).Contains(d));
                    Transform door = doors.ElementAt(random.Next(doors.Count())).Transform;
                    position = door.position + Vector3.up;
                    position += Room.GetRoomAtPosition(position + door.forward.normalized) == ev.Player.Room ? door.forward.normalized : -door.forward.normalized;
                    fallingZombies.Add(ev.Player);
                }
                ev.Player.DisableEffect<PitDeath>();
                ev.Player.Position = position;
                Log.Debug($"Teleported player {ev.Player.Nickname} to safe position after jumping into a void or being crushed by an elevator.", Config.Debug);
                ev.IsAllowed = false;
            }
        }

        private void ApplyEffects(Player player, Dictionary<string, EffectParameters> effects)
        {
            effects?.ForEach(e =>
            {
                if (player.ReferenceHub.playerEffectsController.TryGetEffect(e.Key, out StatusEffectBase effectBase))
                {
                    player.EnableEffect(effectBase, e.Value.Intensity, e.Value.Duration);
                    Log.Debug($"Enabled effect {e.Key} for player {player.Nickname} after walking into a tesla gate or being crushed.", Config.Debug);
                }
            });
        }

        private static readonly System.Random random = new();
        private readonly HashSet<Player> fallingZombies = new();

        public override string Name { get; } = "AntiSuicidalZombies";
        public override string Description { get; } = null;
        public override string Author { get; } = "Catiatto";
        public override Version Version { get; } = new(3, 0, 3);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
    }
}
