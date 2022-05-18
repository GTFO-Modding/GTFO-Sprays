using GTFO.API;
using HarmonyLib;
using Player;
using SNetwork;
using Sprays.Net.Packets;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sprays
{
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyPatch(typeof(SNet_GlobalManager), nameof(SNet_GlobalManager.OnSessionPlayerAction))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        public static void OnSessionPlayerAction_Postfix(SNet_Player player, eSessionPlayerActionType type)
        {
            // OnSessionPlayerAction should only be invoked on master
            Debug.Assert(SNet.IsMaster, "OnSessionPlayerAction invoked, but I'm not master");

            L.Verbose($"OnSessionPlayerAction: {player.NickName} -> {type}");
            switch (type)
            {
                case eSessionPlayerActionType.SpawnPlayerAgent:
                    // Ensure we are not spawning ourselves
                    if (!player.IsMaster)
                    {
                        L.Verbose($"Sending {player.NickName} ({player.Lookup}), our (master's) spray list");
                        var localSprayIdentities = RuntimeLookup.LocalSprays.Select((x) => x.Identity).ToArray();
                        SendSprayList.Instance.Send(new()
                        {
                            length = (byte)localSprayIdentities.Length,
                            sprays = localSprayIdentities,
                        }, player);

                        L.Verbose($"Allowing {player.NickName} ({player.Lookup}) to send their spray list");
                        AllowSendSprayList.Instance.Send(new()
                        {
                            allowedUser = player.Lookup
                        });
                    }
                    break;
            }
        }
    }
}
