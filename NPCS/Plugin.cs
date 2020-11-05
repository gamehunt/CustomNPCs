using Exiled.API.Enums;
using Exiled.API.Features;
using NPCS.AI;
using NPCS.AI.TargetFilters;
using NPCS.Conditions;
using NPCS.Navigation;
using NPCS.Talking;
using System;
using System.IO;
using System.Reflection;
using Evs = Exiled.Events;
using Handlers = Exiled.Events.Handlers;

namespace NPCS
{
    public class Plugin : Exiled.API.Features.Plugin<NPCS.Config>
    {
        //Instance variable for eventhandlers
        public EventHandlers EventHandlers;

        public override string Author { get; } = "gamehunt";
        public override string Name { get; } = "CustomNPCs";
        public override string Prefix { get; } = "CNPCS";
        public override Version Version { get; } = new Version(1, 4, 13);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 13);

        public override PluginPriority Priority => PluginPriority.Lower;

        public HarmonyLib.Harmony Harmony { get; private set; }

        public static Plugin Instance { get; private set; }

        public static Random Random { get; private set; }

        private int reloads = 0;

        public override void OnEnabled()
        {
            try
            {
                Log.Info("Started plugin initialization...");

                Instance = this;

                Random = new Random();

                
                foreach (MethodBase bas in Evs.Events.Instance.Harmony.GetPatchedMethods())
                {
                    if (bas.Name.Equals("TransmitData"))
                    {
                        Exiled.Events.Events.DisabledPatchesHashSet.Add(bas);
                    }
                    else if (bas.DeclaringType.Name.Equals("RoundSummary") && bas.Name.Equals("Start"))
                    {
                        Exiled.Events.Events.DisabledPatchesHashSet.Add(bas);
                    }
                    else if (bas.Name.Equals("BanUser"))
                    {
                        Exiled.Events.Events.DisabledPatchesHashSet.Add(bas);
                    }
                    else if (bas.Name.Equals("CallCmdShoot"))
                    {
                        Exiled.Events.Events.DisabledPatchesHashSet.Add(bas);
                    }
                }

                Evs.Events.Instance.ReloadDisabledPatches();
                

                Harmony = new HarmonyLib.Harmony($"gamehunt.cnpcs.{reloads}");
                reloads++;

                Harmony.PatchAll();

                EventHandlers = new EventHandlers();

                Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
                Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
                Handlers.Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
                Handlers.Server.RespawningTeam += EventHandlers.OnTeamRespawning;

                Handlers.Player.Dying += EventHandlers.OnDying;
                Handlers.Player.EnteringPocketDimension += EventHandlers.OnEnteringPocketDim;
                Handlers.Player.Hurting += EventHandlers.OnHurt;

                Handlers.Map.ExplodingGrenade += EventHandlers.OnGrenadeExplosion;
                Handlers.Map.Decontaminating += EventHandlers.OnDecontamination;

                Handlers.Warhead.Starting += EventHandlers.OnWarheadStart;

                if (!Directory.Exists(Config.NPCs_root_path))
                {
                    Directory.CreateDirectory(Config.NPCs_root_path);
                }
                if (!Directory.Exists(Config.NPCs_nodes_path))
                {
                    Directory.CreateDirectory(Config.NPCs_nodes_path);
                }
                if (!Directory.Exists(Config.NPCs_mappings_path))
                {
                    Directory.CreateDirectory(Config.NPCs_mappings_path);
                }
                if (!File.Exists(Config.NPCs_nav_mappings_path))
                {
                    StreamWriter sw = File.CreateText(Config.NPCs_nav_mappings_path);
                    sw.Write(Config.DefaultNavMappings);
                    sw.Close();
                }
                if (!File.Exists(Path.Combine(Config.NPCs_nodes_path, "default_node.yml")))
                {
                    StreamWriter sw = File.CreateText(Path.Combine(Config.NPCs_nodes_path, "default_node.yml"));
                    sw.Write(Config.DefaultNodeContents);
                    sw.Close();
                }
                if (!File.Exists(Path.Combine(Config.NPCs_root_path, "default_npc.yml")))
                {
                    StreamWriter sw = File.CreateText(Path.Combine(Config.NPCs_root_path, "default_npc.yml"));
                    sw.Write(Config.DefaultNPCContents);
                    sw.Close();
                }

                Log.Info("Registering conditions...");

                NodeCondition.Register(new RoleCondition());
                NodeCondition.Register(new HasItemCondition());
                NodeCondition.Register(new HasntItemCondition());
                NodeCondition.Register(new HealthCondition());
                NodeCondition.Register(new ArtificalHealthCondition());
                NodeCondition.Register(new HasEffectCondition());
                NodeCondition.Register(new HasntEffectCondition());
                NodeCondition.Register(new PermissionCondition());
                NodeCondition.Register(new RoundTimeCondition());
                NodeCondition.Register(new NukeStateCondition());
                NodeCondition.Register(new HeldItemCondition());
                NodeCondition.Register(new RoleExistsCondition());
                NodeCondition.Register(new RoleNotExistsCondition());
                NodeCondition.Register(new RandomCondition());

                Log.Info("Registering actions...");

                NodeAction.Register(new Actions.DieAction());
                NodeAction.Register(new Actions.GiveItemAction());
                NodeAction.Register(new Actions.RetrieveItemAction());
                NodeAction.Register(new Actions.GiveHealthAction());
                NodeAction.Register(new Actions.ConsumeHealthAction());
                NodeAction.Register(new Actions.EnableEffectAction());
                NodeAction.Register(new Actions.DisableEffectAction());
                NodeAction.Register(new Actions.BroadcastAction());
                NodeAction.Register(new Actions.HeldItemAction());
                NodeAction.Register(new Actions.BlackoutAction());
                NodeAction.Register(new Actions.ChangeRoleAction());
                NodeAction.Register(new Actions.ChangeSelfRoleAction());
                NodeAction.Register(new Actions.NukeStartAction());
                NodeAction.Register(new Actions.NukeStopAction());
                NodeAction.Register(new Actions.StartDecontaminationAction());
                NodeAction.Register(new Actions.SwitchGodAction());
                NodeAction.Register(new Actions.MoveAction());
                NodeAction.Register(new Actions.RotationAction());
                NodeAction.Register(new Actions.RoundEndAction());
                NodeAction.Register(new Actions.CassieAction());
                NodeAction.Register(new Actions.RoomTeleportAction());
                NodeAction.Register(new Actions.RoomSelfTeleportAction());
                NodeAction.Register(new Actions.DropItemAction());
                NodeAction.Register(new Actions.NavEnqueueAction());
                NodeAction.Register(new Actions.NavClearAction());
                NodeAction.Register(new Actions.FollowAction());
                NodeAction.Register(new Actions.StopFollowAction());
                NodeAction.Register(new Actions.FireEventAction());
                NodeAction.Register(new Actions.ShootAction());
                NodeAction.Register(new Actions.GoToRoomAction());
                NodeAction.Register(new Actions.ControlDoorAction());
                NodeAction.Register(new Actions.ToggleAIAction());

                Log.Info("Registering AI targets...");

                AITarget.Register(new AITestTarget());
                AITarget.Register(new AIFindPlayerTarget());
                AITarget.Register(new AIShootTarget());
                AITarget.Register(new AINavigateToRoom());
                AITarget.Register(new AIFollowTarget());
                AITarget.Register(new AIFindItemTarget());
                AITarget.Register(new AIConditionalJump());
                AITarget.Register(new AIRunAwayTarget());

                Log.Info("Registering targets filters...");

                TargetFilter.Register(new CommonTargetFilter());
                TargetFilter.Register(new Scp939TargetFilter());
                TargetFilter.Register(new Scp096TargetFilter());

                Log.Info($"CustomNPCs plugin loaded. @gamehunt");
            }
            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            foreach (Npc npc in Npc.List)
            {
                npc.Kill(false);
            }

            Harmony.UnpatchAll();

            NavigationNode.Clear();
            NodeCondition.Clear();
            NodeAction.Clear();
            AITarget.Clear();
            TargetFilter.Clear();

            Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
            Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
            Handlers.Server.RespawningTeam -= EventHandlers.OnTeamRespawning;

            Handlers.Player.Dying -= EventHandlers.OnDying;
            Handlers.Player.EnteringPocketDimension -= EventHandlers.OnEnteringPocketDim;
            Handlers.Player.Hurting -= EventHandlers.OnHurt;

            Handlers.Map.ExplodingGrenade -= EventHandlers.OnGrenadeExplosion;
            Handlers.Map.Decontaminating -= EventHandlers.OnDecontamination;

            Handlers.Warhead.Starting -= EventHandlers.OnWarheadStart;

            EventHandlers = null;
            Random = null;
            Harmony = null;
            Instance = null;
        }

        public override void OnReloaded()
        {
            NavigationNode.Clear();
            NodeCondition.Clear();
            NodeAction.Clear();
            AITarget.Clear();
            TargetFilter.Clear();
        }
    }
}