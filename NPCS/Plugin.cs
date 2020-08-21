using Exiled.API.Enums;
using Exiled.API.Features;
using NPCS.Conditions;
using NPCS.Talking;
using System;
using System.Collections.Generic;
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
        public override Version Version { get; } = new Version(1, 2, 3);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 0);

        public override PluginPriority Priority => PluginPriority.Last;

        public HarmonyLib.Harmony harmony;

        public static Plugin Instance { get; private set; }

        public override void OnEnabled()
        {
            try
            {
                Instance = this;

                if (!Config.IsEnabled)
                {
                    Log.Info("CustomNPCs is disabled");
                    return;
                }

                //Events.DisabledPatches.Add(new Tuple<Type, string>(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData)));
                //Events.Instance.ReloadDisabledPatches(); - NullReferenceException mindfuck

                //F u c k

                List<MethodBase> methods = new List<MethodBase>(Evs.Events.Instance.Harmony.GetPatchedMethods());
                foreach (System.Reflection.MethodBase bas in methods)
                {
                    var info = HarmonyLib.Harmony.GetPatchInfo(bas);
                    if (bas.Name.Equals("TransmitData"))
                    {
                        Evs.Events.Instance.Harmony.Unpatch(bas, HarmonyLib.HarmonyPatchType.All, Evs.Events.Instance.Harmony.Id);
                        Log.Info("Unpatched GhostMode");
                    }
                    else if (bas.DeclaringType.Name.Equals("RoundSummary") && bas.Name.Equals("Start"))
                    {
                        Evs.Events.Instance.Harmony.Unpatch(bas, HarmonyLib.HarmonyPatchType.All, Evs.Events.Instance.Harmony.Id);
                        Log.Info("Unpatched RoundSummary.Start");
                    }
                    else if (bas.DeclaringType.Name.Equals("ReferenceHub") && bas.Name.Equals("OnDestroy"))
                    {
                        Evs.Events.Instance.Harmony.Unpatch(bas, HarmonyLib.HarmonyPatchType.All, Evs.Events.Instance.Harmony.Id);
                        Log.Info("Unpatched ReferenceHub.OnDestroy");
                    }
                    else if (bas.Name.Equals("BanUser"))
                    {
                        Evs.Events.Instance.Harmony.Unpatch(bas, HarmonyLib.HarmonyPatchType.All, Evs.Events.Instance.Harmony.Id);
                        Log.Info("Unpatched BanPlayer.BanUser");
                    }
                }

                harmony = new HarmonyLib.Harmony("gamehunt.cnpcs");
                harmony.PatchAll();

                EventHandlers = new EventHandlers(this);

                Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
                Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;

                Handlers.Player.Died += EventHandlers.OnDied;

                Handlers.Map.ExplodingGrenade += EventHandlers.OnGrenadeExplosion;
                Handlers.Map.Decontaminating += EventHandlers.OnDecontamination;

                if (!Directory.Exists(Config.NPCs_root_path))
                {
                    Directory.CreateDirectory(Config.NPCs_root_path);
                }
                if (!Directory.Exists(Config.NPCs_nodes_path))
                {
                    Directory.CreateDirectory(Config.NPCs_nodes_path);
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
                NodeAction.Register(new Actions.DropItemAction());

                Log.Info($"CustomNPCs plugin loaded. @gamehunt");
            }
            catch (Exception e)
            {
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;

            Handlers.Player.Died -= EventHandlers.OnDied;

            Handlers.Map.ExplodingGrenade -= EventHandlers.OnGrenadeExplosion;
            Handlers.Map.Decontaminating -= EventHandlers.OnDecontamination;

            EventHandlers = null;
        }

        public override void OnReloaded()
        {
        }
    }
}