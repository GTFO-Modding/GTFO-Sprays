using SNetwork;
using Sprays.Net.Models;
using Sprays.Net.Packets.TextureTransport;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pSprayList
    {
        public byte length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public pSprayIdentityInfo[] sprays;
    }
    internal class SendSprayList : BasePacket<SendSprayList, pSprayList>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(SendSprayList)}";
        public static void Register() => new SendSprayList().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pSprayList payload)
        {
            for (int i = 0; i < payload.length; i++)
            {
                pSprayIdentityInfo sprayIdentity = payload.sprays[i];
                if (sprayIdentity == default) continue;

                string strChecksum = Utilities.StringUtils.FromByteArrayAsHex(sprayIdentity.ChecksumData);
                L.Verbose($"{sender.NickName} ({sender.Lookup})'s spray {i} checksum: {strChecksum}");

                Spray spray = sprayIdentity.SprayObject;
                if (spray != null)
                {
                    L.Verbose($"{sender.NickName} ({sender.Lookup})'s spray {i} already exists locally. Not requesting.");
                    return;
                }
                spray = Cache.LoadSprayByChecksum(strChecksum);
                if (spray != null)
                {
                    L.Verbose($"{sender.NickName} ({sender.Lookup})'s spray {i} has been obtained from the cache.");
                    return;
                }
                L.Verbose($"{sender.NickName} ({sender.Lookup})'s spray {i} doesn't exist locally. Requesting");
                RequestTextureData.Instance.Send(new()
                {
                    cookie = TextureDataChunker.GetNewCookie(sprayIdentity),
                    spray = sprayIdentity
                }, sender);
            }
        }
    }
}
