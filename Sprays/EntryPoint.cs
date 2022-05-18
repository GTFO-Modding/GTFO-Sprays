using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using SNetwork;
using Sprays.Net.Packets;
using Sprays.Net.Packets.TextureTransport;
using Sprays.Resources;
using System.IO;
using UnityEngine;

namespace Sprays
{
    [BepInPlugin("com.mccad00.Sprays", "Sprays", "1.0.0")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    public class EntryPoint : BasePlugin
    {
        // The method that gets called when BepInEx tries to load our plugin
        public override void Load()
        {
            m_Harmony = new Harmony("com.mccad00.Sprays");
            m_Harmony.PatchAll();
            SetupConfig(base.Config);

            // Texture Transport
            CookieReady.Register();
            CookieTextureData.Register();
            RequestTextureData.Register();
            TextureDataChunk.Register();

            AllowSendSprayList.Register();
            ApplySpray.Register();
            SendSprayList.Register();

            AssetAPI.OnImplReady += () =>
            {
                // TODO: Separate
                var spraysPath = Path.Combine(Paths.ConfigPath, "sprays");
                if (!Directory.Exists(spraysPath)) Directory.CreateDirectory(spraysPath);

                foreach (string sprayFile in Directory.EnumerateFiles(spraysPath, "*.png"))
                {
                    Spray localSpray = Spray.FromFile(sprayFile);
                    RuntimeLookup.Sprays.Add(localSpray);
                    RuntimeLookup.LocalSprays.Add(localSpray);
                }
            };

            //NetworkAPI.RegisterEvent<int>("ClearPlayerSpray", NetworkedSprays.OnReceiveClearSprayData);
            //NetworkAPI.RegisterEvent<NetworkedSprays.pReceiveSprayData>("ApplyPlayerSpray", NetworkedSprays.OnReceiveApplySpray);
            //NetworkAPI.RegisterEvent<NetworkedSprays.pSprayData>("PostSprayData", NetworkedSprays.OnReceiveSprayData);
            //NetworkAPI.RegisterEvent<byte>("RequestSprayData", NetworkedSprays.OnReceiveSprayDataRequest);
            //NetworkAPI.RegisterEvent<byte>("ClientReplySprayData", NetworkedSprays.OnReceivePostSprayRequest);

            AddComponent<SprayInputHandler>();
        }

        private static void SetupConfig(ConfigFile config)
        {
            SprayKey = config.Bind("Keybinds", "Apply Spray", KeyCode.G);
        }

        public static ConfigEntry<KeyCode> SprayKey;
        private Harmony m_Harmony;
    }
}
