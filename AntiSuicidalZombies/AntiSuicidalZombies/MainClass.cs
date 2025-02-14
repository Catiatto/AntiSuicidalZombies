using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomPlayerEffects;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MapGeneration;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

namespace AntiSuicidalZombies
{
    public class MainClass : Plugin<Config>
    {
        public override void Enable()
        {
            PlayerEvents.Hurting += OnPlayerHurting;
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= OnPlayerHurting;
        }

        internal void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Target.Role != RoleTypeId.Scp0492 || ev.DamageHandler is not UniversalDamageHandler udh)
            {
                return;
            }
            if (udh.TranslationId == DeathTranslations.Tesla.Id && Config.TeslaEffects != null)
            {
                foreach (var effect in Config.TeslaEffects)
                {
                    if (ev.Target.ReferenceHub.playerEffectsController.TryGetEffect(effect.Key, out StatusEffectBase effectBase))
                    {
                        ev.Target.EnableEffect(effectBase, effect.Value.Intensity, effect.Value.Duration);
                    }
                }
                Log.Debug($"Enabled effects for player {ev.Target.Nickname} after walking into a tesla gate.", Config.Debug);
                ev.IsAllowed = false;
            }
            if (udh.TranslationId == DeathTranslations.Crushed.Id)
            {
                if (udh.Damage == 200f && Config.CrushedEffects != null)
                {
                    foreach (var effect in Config.CrushedEffects)
                    {
                        if (ev.Target.ReferenceHub.playerEffectsController.TryGetEffect(effect.Key, out StatusEffectBase effectBase))
                        {
                            ev.Target.EnableEffect(effectBase, effect.Value.Intensity, effect.Value.Duration);
                        }
                    }
                    Log.Debug($"Enabled effects for player {ev.Target.Nickname} after being crushed by a bulk door.", Config.Debug);
                    ev.IsAllowed = false;
                    return;
                }
                Vector3 position;
                if (ev.Target.Room == null)
                {
                    position = AlphaWarheadNukesitePanel.Singleton.transform.position + 3 * Vector3.up;
                }
                else if (ev.Target.Room.Name == RoomName.HczWarhead)
                {
                    if (Config.CrushedEffects != null)
                    {
                        foreach (var effect in Config.CrushedEffects)
                        {
                            if (ev.Target.ReferenceHub.playerEffectsController.TryGetEffect(effect.Key, out StatusEffectBase effectBase))
                            {
                                ev.Target.EnableEffect(effectBase, effect.Value.Intensity, effect.Value.Duration);
                            }
                        }
                    }
                    IEnumerable<Vector3> elevators = Elevator.GetByGroup(ElevatorGroup.Nuke01).Concat(Elevator.GetByGroup(ElevatorGroup.Nuke02)).Select(e => e.Base.transform.position);
                    position = Vector3.Distance(ev.Target.Position, elevators.ElementAt(0)) < Vector3.Distance(ev.Target.Position, elevators.ElementAt(1)) ? elevators.ElementAt(0) : elevators.ElementAt(1);
                    position += 2 * Vector3.up;
                }
                else
                {
                    //IEnumerable<Door> doors = ev.Target.Room.Doors.Where(d => d.Permissions == KeycardPermissions.None);
                    IEnumerable<Door> doors = Door.List.Where(d => d.Rooms.Contains(ev.Target.Room.Base) && d.Permissions == KeycardPermissions.None);
                    Door door = doors.ElementAt(random.Next(doors.Count()));
                    position = door.Position + Vector3.up;
                    position += Room.GetRoomAtPosition(position + door.Transform.forward.normalized) == ev.Target.Room ? door.Transform.forward.normalized : -door.Transform.forward.normalized;
                }
                ev.Target.DisableEffect<PitDeath>();
                ev.Target.Position = position;
                Log.Debug($"Teleported player {ev.Target.Nickname} to safe position after jumping into a void or being crushed by an elevator.", Config.Debug);
                ev.IsAllowed = false;
            }
        }

        private static readonly System.Random random = new();

        public override string Name { get; } = "AntiSuicidalZombies";
        public override string Description { get; } = null;
        public override string Author { get; } = "Phineapple18";
        public override Version Version { get; } = new(3, 0, 0);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
    }
}
