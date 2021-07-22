using System.Linq;
using System.Text;
using Exiled.Events.EventArgs;
using Exiled.Events.Handlers;
using HarmonyLib;
using NorthwoodLib.Pools;
using Respawning.NamingRules;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement))]
	internal class EntranceAnnouncementFix
    {
        private static bool Prefix(NineTailedFoxNamingRule __instance, string regular)
        {
			int num = (from x in ReferenceHub.GetAllHubs().Values
					   where x.characterClassManager.CurRole.team == Team.SCP 
					   && x.characterClassManager.CurClass != RoleType.Scp0492 
					   && !Npc.Dictionary.ContainsKey(x.gameObject)
					   select x).Count();

			string[] unitInformations = regular.Split('-');

			var ev = new AnnouncingNtfEntranceEventArgs(num, unitInformations[0], int.Parse(unitInformations[1]));

			Map.OnAnnouncingNtfEntrance(ev);

			regular = $"{ev.UnitName}-{ev.UnitNumber}";

			string cassieUnitName = __instance.GetCassieUnitName(regular);

			StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
			if (global::ClutterSpawner.IsHolidayActive(global::Holidays.Christmas))
			{
				stringBuilder.Append("XMAS_EPSILON11 ");
				stringBuilder.Append(cassieUnitName);
				stringBuilder.Append("XMAS_HASENTERED ");
				stringBuilder.Append(num);
				stringBuilder.Append(" XMAS_SCPSUBJECTS");
			}
			else
			{
				stringBuilder.Append("MTFUNIT EPSILON 11 DESIGNATED ");
				stringBuilder.Append(cassieUnitName);
				stringBuilder.Append(" HASENTERED ALLREMAINING ");
				if (num == 0)
				{
					stringBuilder.Append("NOSCPSLEFT");
				}
				else
				{
					stringBuilder.Append("AWAITINGRECONTAINMENT ");
					stringBuilder.Append(num);
					if (num == 1)
					{
						stringBuilder.Append(" SCPSUBJECT");
					}
					else
					{
						stringBuilder.Append(" SCPSUBJECTS");
					}
				}
			}
			__instance.ConfirmAnnouncement(ref stringBuilder);
			StringBuilderPool.Shared.Return(stringBuilder);
			return false;
		}
    }
}
