using SNetwork;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Packets.TextureTransport
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct pTextureDataChunk
    {
        public byte cookie;
        public ushort chunkIdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.TEXTURE_CHUNKSIZE)]
        public byte[] chunkData;

        // not needed because we have cookies, but keep for sanity
        [MarshalAs(UnmanagedType.I1)]
        public bool isFinal;
    }
    internal class TextureDataChunk : BasePacket<TextureDataChunk, pTextureDataChunk>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(TextureDataChunk)}";
        public static void Register() => new TextureDataChunk().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pTextureDataChunk payload)
        {
            TextureDataChunker.ReceiveChunk(sender, payload);
        }
    }
}
