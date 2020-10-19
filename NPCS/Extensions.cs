using Exiled.API.Extensions;
using Exiled.API.Features;
using System;

namespace NPCS
{
    public static class Extensions
    {
        public static bool IsNPC(this Player p)
        {
            return Npc.Dictionary.ContainsKey(p.GameObject);
        }

        public static bool IsClosed(this Lift lift, Lift.Elevator elev)
        {
            for (int i = 0; i < lift.elevators.Length; i++)
            {
                if (lift.elevators[i] == elev)
                {
                    if (i == 0)
                    {
                        return lift.NetworkstatusID == (int)Lift.Status.Down;
                    }
                    else
                    {
                        return lift.NetworkstatusID == (int)Lift.Status.Up;
                    }
                }
            }
            return lift.NetworkstatusID != (int)Lift.Status.Moving;
        }

        private static bool CanOpen(Door.AccessRequirements level, ItemType keycard)
        {
            if (!keycard.IsKeycard())
            {
                return false;
            }
            switch (level)
            {
                case Door.AccessRequirements.AlphaWarhead:
                    return keycard == ItemType.KeycardO5 || keycard == ItemType.KeycardFacilityManager || keycard == ItemType.KeycardContainmentEngineer;

                case Door.AccessRequirements.ArmoryLevelOne:
                    return ItemType.KeycardGuard <= keycard && keycard != ItemType.KeycardContainmentEngineer && keycard != ItemType.KeycardFacilityManager;

                case Door.AccessRequirements.ArmoryLevelTwo:
                    return ItemType.KeycardSeniorGuard <= keycard && keycard != ItemType.KeycardContainmentEngineer && keycard != ItemType.KeycardFacilityManager;

                case Door.AccessRequirements.ArmoryLevelThree:
                    return ItemType.KeycardNTFCommander <= keycard && keycard != ItemType.KeycardFacilityManager;

                case Door.AccessRequirements.Checkpoints:
                    return keycard != ItemType.KeycardJanitor && keycard != ItemType.KeycardScientist;

                case Door.AccessRequirements.ContainmentLevelOne:
                    return true;

                case Door.AccessRequirements.ContainmentLevelTwo:
                    return keycard >= ItemType.KeycardScientist && keycard != ItemType.KeycardZoneManager && keycard != ItemType.KeycardGuard;

                case Door.AccessRequirements.ContainmentLevelThree:
                    return keycard == ItemType.KeycardContainmentEngineer || keycard == ItemType.KeycardO5;

                case Door.AccessRequirements.Intercom:
                    return keycard >= ItemType.KeycardNTFCommander || keycard == ItemType.KeycardContainmentEngineer;

                case Door.AccessRequirements.Gates:
                    return keycard >= ItemType.KeycardContainmentEngineer;

                default:
                    return false;
            }
        }

        public static bool CanBeOpenedWith(this Door door, ItemType keycard)
        {
            foreach (Door.AccessRequirements level in Enum.GetValues(typeof(Door.AccessRequirements)))
            {
                if (door.PermissionLevels.HasPermission(level) && !CanOpen(level, keycard))
                {
                    return false;
                }
            }
            return true;
        }
    }
}