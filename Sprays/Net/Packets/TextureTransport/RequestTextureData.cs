using SNetwork;
using Sprays.Net.Models;
using Sprays.Net.Packets.TextureTransport;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets.TextureTransport
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pRequestTextureData
    {
        public byte cookie;
        public pSprayIdentityInfo spray;
    }
    internal class RequestTextureData : BasePacket<RequestTextureData, pRequestTextureData>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(RequestTextureData)}";
        public static void Register() => new RequestTextureData().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pRequestTextureData payload)
        {
            string strChecksum = Utilities.StringUtils.FromByteArrayAsHex(payload.spray.ChecksumData);
            L.Verbose($"{sender.NickName} ({sender.Lookup}) requested the spray texture data for spray {strChecksum}");

            var localSpray = payload.spray.LocalSprayObject;
            if (localSpray == null)
            {
                L.Error($"Requested spray '{strChecksum}' texture requested from {sender.NickName} ({sender.Lookup}) doesn't exist locally." +
                    $"Exists from external sender: {payload.spray.SprayObject != null}");
                return;
            }

            CookieTextureData.Instance.Send(new() { 
                cookie = payload.cookie,
                textureSize = (uint)localSpray.TextureData.Length
            }, sender);
        }
    }
}
