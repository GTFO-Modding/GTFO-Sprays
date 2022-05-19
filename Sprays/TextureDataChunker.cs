using SNetwork;
using Sprays.Net.Models;
using Sprays.Net.Packets.TextureTransport;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Sprays
{
    internal static class TextureDataChunker
    {
        public static ushort GetChunksNeeded(uint length) => (ushort)Math.Ceiling(length / (decimal)Constants.TEXTURE_CHUNKSIZE);

        private class TextureCookieInfo
        {
            // Unmodified
            public byte cookie;
            public pSprayIdentityInfo spray;
            public ushort expectedChunks;

            // Volatile
            public DateTimeOffset lastModified;
            public byte[] textureBuffer;
            public uint receivedChunkSum;

            public bool isComplete = false;
        }
        public static void ReceiveChunk(SNet_Player sender, pTextureDataChunk chunk)
        {
            if (!s_CookieLookup.TryGetValue(chunk.cookie, out TextureCookieInfo cookieInfo))
            {
                // This is usually fatal, but don't log anything because it'll be fatal
                return;
            }

            if (chunk.chunkIdx >= cookieInfo.expectedChunks)
            {
                L.Error($"Received chunk index {chunk.chunkIdx} from {sender.NickName} ({sender.Lookup}), but we expecetd {cookieInfo.expectedChunks}");
                return;
            }

            Unsafe.CopyBlock(ref cookieInfo.textureBuffer[chunk.chunkIdx * Constants.TEXTURE_CHUNKSIZE], ref chunk.chunkData[0], (uint)Math.Min(cookieInfo.textureBuffer.Length - (chunk.chunkIdx * Constants.TEXTURE_CHUNKSIZE), chunk.chunkData.Length));

            cookieInfo.lastModified = DateTimeOffset.UtcNow;
            cookieInfo.receivedChunkSum += chunk.chunkIdx;

            if (!chunk.isFinal) return;
            uint expectedChunkSum = (uint)Enumerable.Sum(Enumerable.Range(0, cookieInfo.expectedChunks).Select((x) => (long)x));

            if (expectedChunkSum != cookieInfo.receivedChunkSum)
            {
                L.Error($"Chunk sum {sender.NickName} ({sender.Lookup}) mismatch ({expectedChunkSum} != {cookieInfo.receivedChunkSum}). Cookie {cookieInfo.cookie}");
                return;
            }

            // NOTE: We should probably not do this here
            L.Verbose($"Building spray with finalized cookie {cookieInfo.cookie} buffer from {sender.NickName} ({sender.Lookup})");
            RuntimeLookup.Sprays.Add(Spray.FromBytes(cookieInfo.textureBuffer));

            s_CookieLookup[cookieInfo.cookie] = null;
            GC.Collect();
        }
        public static bool SetupCookie(SNet_Player sender, pCookieTextureData textureData)
        {
            if(!s_CookieLookup.TryGetValue(textureData.cookie, out TextureCookieInfo cookieInfo))
            {
                L.Error($"Cookie Texture Data received from {sender.NickName} ({sender.Lookup}) pointed to an invalid cookie with identifier {textureData.cookie}." +
                    $"Texture size: {textureData.textureSize}");
                return false;
            }

            if(textureData.textureSize > Constants.MAX_TEXTURE_SIZE)
            {
                L.Error($"Cookie Texture Data received from {sender.NickName} ({sender.Lookup}) exceeds the maximum texture size. ({textureData.textureSize} > {Constants.MAX_TEXTURE_SIZE})");
                return false;
            }

            cookieInfo.textureBuffer = new byte[textureData.textureSize];
            cookieInfo.expectedChunks = GetChunksNeeded(textureData.textureSize);
            cookieInfo.lastModified = DateTimeOffset.UtcNow;

            return true;
        }
        public static pSprayIdentityInfo GetSprayFromCookie(byte cookie) => s_CookieLookup[cookie].spray;
        public static byte GetNewCookie(pSprayIdentityInfo identity)
        {
            byte cookieIdentifier = GetNewCookieIdentifier();
            s_CookieLookup[cookieIdentifier] = new()
            {
                cookie = cookieIdentifier,
                spray = identity,
                lastModified = DateTimeOffset.UtcNow,
                // Created when texture info is received
                textureBuffer = null
            };
            return cookieIdentifier;
        }
        private static byte GetNewCookieIdentifier()
        {
            int cookie = Interlocked.Increment(ref s_LastCookie) & 0x0F;
            return Unsafe.As<int, byte>(ref cookie);
        }
        private static readonly Dictionary<byte, TextureCookieInfo> s_CookieLookup = new();
        private static int s_LastCookie;
    }
}
