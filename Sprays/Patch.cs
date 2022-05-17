using GTFO.API;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprays
{
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerSync), nameof(PlayerSync.OnSpawn))]
        public static void OnSpawn()
        {
            L.Debug("Player spawned, setting up sprays");
            NetworkedSprays.Setup();
            NetworkedSprays.Current.PostLocalSprayData();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.RegisterPlayerAgent))]
        public static void OnRegisterPlayerAgent()
        {
            L.Debug("Player registered, setting up sprays");
            NetworkedSprays.Setup();
            NetworkedSprays.Current.PostLocalSprayData();
            NetworkAPI.InvokeEvent("RequestSprayData", 0);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UnregisterPlayerAgent))]
        public static void OnUnregisterPlayerAgent()
        {
            L.Debug("Player registered, setting up sprays");
            NetworkedSprays.Setup();
            NetworkedSprays.Current.PostLocalSprayData();
        }
    }
}
