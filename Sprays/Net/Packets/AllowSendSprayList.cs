using SNetwork;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pAllowSendSprayList
    {
        public ulong allowedUser;
    }
    internal class AllowSendSprayList : BasePacket<AllowSendSprayList, pAllowSendSprayList>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(AllowSendSprayList)}";
        public static void Register() => new AllowSendSprayList().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pAllowSendSprayList payload)
        {
            // AllowSendSprayList is sent to a specific client, but the client's id
            // is also specified in the payload just in case :)
            if (SNet.LocalPlayer.Lookup != payload.allowedUser) return;
            if (!sender.IsMaster)
            {
                L.Warn($"Non-Master {sender.NickName} ({sender.Lookup}) allowed me to send spray list. Ignoring");
                return;
            }

            L.Verbose($"Sending spray list to lobby members");
            var localSprayIdentities = RuntimeLookup.LocalSprays.Select((x) => x.Identity).ToArray();
            SendSprayList.Instance.Send(new() {
                length = (byte)localSprayIdentities.Length,
                sprays = localSprayIdentities,
            });
        }
    }
}
