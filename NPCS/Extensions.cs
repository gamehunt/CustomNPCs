using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS
{
    public static class Extensions
    {
        public static List<Door> GetDoors(this Room room)
        {
            List<Door> list2 = new List<Door>();
            foreach (global::Scp079Interactable scp079Interactable2 in global::Interface079.singleton.allInteractables)
            {
                foreach (global::Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable2.currentZonesAndRooms)
                {
                    if (zoneAndRoom.currentRoom == room.Name && zoneAndRoom.currentZone == room.Transform.parent.name)
                    {
                        if (scp079Interactable2.type == Scp079Interactable.InteractableType.Door)
                        {
                            Door door = scp079Interactable2.GetComponent<Door>();
                            if (!list2.Contains(door))
                            {
                                list2.Add(door);
                            }
                        }
                    }
                }
            }
            return list2;
        }
    }
}