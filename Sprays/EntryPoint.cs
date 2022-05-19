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
using System.Reflection;
using UnityEngine;
using System.Linq;

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
                if (!Directory.Exists(spraysPath))
                {
                    Directory.CreateDirectory(spraysPath);
                    var assembly = Assembly.GetExecutingAssembly();

                    using var defaultSpray = assembly.GetManifestResourceStream("Sprays.assets.ExampleSpray.png");
                    using var fileStream = File.OpenWrite(Path.Combine(spraysPath, "ExampleSpray.png"));

                    defaultSpray.CopyTo(fileStream);
                }
                
                var loadedSprays = Directory.GetFiles(spraysPath);

                for (var i = 0; i < Math.Min(loadedSprays.Length, Constants.SPRAYLIST_LIMIT); i++)
                {
                    Spray localSpray = Spray.FromFile(loadedSprays[i]);

                    //RuntimeLookup.Sprays.Add(localSpray);
                    RuntimeLookup.LocalSprays.Add(localSpray);
                }
            };
            SprayInputHandler.Current = AddComponent<SprayInputHandler>();
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<Spray>();
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
