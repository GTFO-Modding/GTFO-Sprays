using SNetwork;
using Sprays.Net.Models;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets.TextureTransport
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pCookieReady
    {
        public byte cookie;
        public pSprayIdentityInfo spray;
        [MarshalAs(UnmanagedType.I1)]
        public bool isReady;
    }
    internal class CookieReady : BasePacket<CookieReady, pCookieReady>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(CookieReady)}";
        public static void Register() => new CookieReady().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pCookieReady payload)
        {
            L.Verbose($"Cookie {payload.cookie} from {sender.NickName} ({sender.Lookup}) is ready?: {payload.isReady}");

            // Cookie owner isn't ready, this could mean that the cookie is expired or that the cookie is invalid
            // Don't send chunks to the player as they will not be listening for them
            if (!payload.isReady) return;

            string strChecksum = Utilities.StringUtils.FromByteArrayAsHex(payload.spray.ChecksumData);

            var localSpray = payload.spray.LocalSprayObject;
            if (localSpray == null)
            {
                // This should never be reached if we are in CookieReady as the cookie wouldn't exist if the spray doesn't exist locally
                L.Error($"Requested spray for chunking '{strChecksum}' texture requested from {sender.NickName} ({sender.Lookup}) doesn't exist locally." +
                    $"Exists from external sender: {payload.spray.SprayObject != null}");
                return;
            }

            byte[] textureData = localSpray.TextureData;
            var chunkCount = TextureDataChunker.GetChunksNeeded((uint)textureData.Length);

            L.Verbose($"Sending {chunkCount} chunks to {sender.NickName} ({sender.Lookup})");

            var chunk = new byte[Constants.TEXTURE_CHUNKSIZE];

            for (ushort i = 0; i < chunkCount; i++)
            {
                Unsafe.CopyBlock(ref chunk[0], ref textureData[i * Constants.TEXTURE_CHUNKSIZE], (uint)Math.Min(Constants.TEXTURE_CHUNKSIZE, textureData.Length - (i * Constants.TEXTURE_CHUNKSIZE)));

                TextureDataChunk.Instance.Send(new()
                {
                    cookie = payload.cookie,
                    chunkIdx = i,
                    chunkData = chunk,
                    isFinal = i + 1 == chunkCount,
                }, sender);
            }
        }
    }
}
