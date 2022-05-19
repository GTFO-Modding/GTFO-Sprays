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
using System;
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
                // TODO: Separate ;)
                var spraysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GTFO-Modding/Frog/Sprays");
                L.Error(spraysPath);
                if (!Directory.Exists(spraysPath)) Directory.CreateDirectory(spraysPath);

                var i = 0;

                foreach (string sprayFile in Directory.EnumerateFiles(spraysPath)) //For loop would be cleaner here but i couldn't figure out how to get the count of EnumerateFiles
                {
                    Spray localSpray = Spray.FromFile(sprayFile);

                    //RuntimeLookup.Sprays.Add(localSpray);
                    RuntimeLookup.LocalSprays.Add(localSpray);
                    i++;

                    if (i >= 10) break;
                }
            };
            SprayInputHandler.Current = AddComponent<SprayInputHandler>();
        }

        private static void SetupConfig(ConfigFile config)
        {
            SprayKey = config.Bind("Keybinds", "Apply Spray", KeyCode.G);
            SprayLimit = config.Bind("Settings", "Maximum Spray Count per Player", 3);
        }

        public static ConfigEntry<KeyCode> SprayKey;
        public static ConfigEntry<int> SprayLimit;
        private Harmony m_Harmony;
    }
}
