using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using GTFO.API;
using HarmonyLib;
using UnityEngine;

namespace Sprays
{
    [BepInPlugin("com.mccad00.Sprays", "Sprays", "1.0.0")]
    public class EntryPoint : BasePlugin
    {
        // The method that gets called when BepInEx tries to load our plugin
        public override void Load()
        {
            m_Harmony = new Harmony("com.mccad00.Sprays");
            m_Harmony.PatchAll();
            SetupConfig(base.Config);

            NetworkAPI.RegisterEvent<int>("ClearPlayerSpray", NetworkedSprays.OnReceiveClearSprayData);
            NetworkAPI.RegisterEvent<NetworkedSprays.pReceiveSprayData>("ApplyPlayerSpray", NetworkedSprays.OnReceiveApplySpray);
            NetworkAPI.RegisterEvent<NetworkedSprays.pSprayData>("PostSprayData", NetworkedSprays.OnReceiveSprayData);
            NetworkAPI.RegisterEvent<byte>("RequestSprayData", NetworkedSprays.OnReceiveSprayDataRequest);
            NetworkAPI.RegisterEvent<byte>("PostSprayData", NetworkedSprays.OnReceivePostSprayRequest);

            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<SprayInputHandler>();
        }

        private static void SetupConfig(ConfigFile config)
        {
            SprayKey = config.Bind("Keybinds", "Apply Spray", KeyCode.G);
        }

        public static ConfigEntry<KeyCode> SprayKey;
        private Harmony m_Harmony;
    }
}
