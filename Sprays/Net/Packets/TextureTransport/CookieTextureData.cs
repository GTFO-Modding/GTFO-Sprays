using SNetwork;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets.TextureTransport
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pCookieTextureData
    {
        public byte cookie;
        public uint textureSize;
    }
    internal class CookieTextureData : BasePacket<CookieTextureData, pCookieTextureData>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(CookieTextureData)}";
        public static void Register() => new CookieTextureData().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pCookieTextureData payload)
        {
            L.Verbose($"Received texture data for cookie {payload.cookie}. Texture size: {payload.textureSize}");
            bool setupSuccess = TextureDataChunker.SetupCookie(sender, payload);
            CookieReady.Instance.Send(new()
            {
                cookie = payload.cookie,
                spray = setupSuccess ? TextureDataChunker.GetSprayFromCookie(payload.cookie) : default,
                isReady = setupSuccess,
            }, sender);
        }
    }
}
