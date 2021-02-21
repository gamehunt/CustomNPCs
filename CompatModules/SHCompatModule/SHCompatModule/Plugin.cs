using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Linq;

namespace SHCompatModule
{
    public class Plugin : Exiled.API.Features.Plugin<SHCompatModule.Config>
    {
        public override string Author { get; } = "gamehunt";
        public override string Name { get; } = "SHCompatModule";
        public override string Prefix { get; } = "SHCompatModule";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 12);

        public NPCS.Plugin NPCPlugin { get; private set; }

        public override PluginPriority Priority => PluginPriority.Last;

        public HarmonyLib.Harmony Harmony { get; private set; }

        public static Plugin Instance { get; private set; }

        public override void OnEnabled()
        {
            try
            {
                Instance = this;

                NPCPlugin = (NPCS.Plugin)Exiled.Loader.Loader.Plugins.Where(p => p.Name == "CustomNPCs").FirstOrDefault();
                if (NPCPlugin == null)
                {
                    Log.Error("Failed to load SH compat module addon: SH not found!");
                    return;
                }

                if (Exiled.Loader.Loader.Plugins.Where(p => p.Name == "SerpentsHand").FirstOrDefault() == null)
                {
                    Log.Error("Failed to load SH compat module addon: SH not found!");
                    return;
                }

                Harmony = new HarmonyLib.Harmony("gamehunt.compat.sh");
                Harmony.PatchAll();

                Log.Info($"SH compat module loaded. @gamehunt");
            }
            catch (Exception e)
            {
                //This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
        }

        public override void OnReloaded()
        {
            //This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
        }
    }
}