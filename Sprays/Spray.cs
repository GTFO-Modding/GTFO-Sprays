using AK;
using BepInEx;
using Decals;
using SNetwork;
using Sprays.Net.Models;
using Sprays.Net.Packets;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Cache = Sprays.Resources.Cache;
using Debug = System.Diagnostics.Debug;

namespace Sprays
{
    internal class SprayInstance
    {
        public SprayInstance(Spray spray)
        {
            m_BackingGameObject = new($"SprayInst-{spray.Checksum}");

            m_BackingGameObject.transform.localScale = Vector3.one * 1.5f;

            m_Decal = m_BackingGameObject.AddComponent<Decal>();
            m_Decal.m_decalMaterial = spray.Material;
            m_Decal.m_decalType = Decal.DecalType.Generic;
        }

        public void ApplyFromPacket(pApplySpray packet)
        {
            m_Decal.transform.position = packet.position;
            m_Decal.transform.localPosition += new Vector3(0, 0, -0.05f);
            m_Decal.transform.rotation = Quaternion.LookRotation(packet.forward);

            m_Decal.m_isRegistered = false;
            m_Decal.ResetMatrix();
            m_Decal.Shown(true);

            Spray.SoundPlayer.Post(EVENTS.MEDSPRAYLOOPEND, packet.position);
        }

        private readonly Decal m_Decal;
        private readonly GameObject m_BackingGameObject;
    }
    internal class Spray
    {
        public static CellSoundPlayer SoundPlayer
        {
            get
            {
                if (s_SoundPlayer == null) s_SoundPlayer = new();
                return s_SoundPlayer;
            }
        }

        public string Checksum => Utilities.StringUtils.FromByteArrayAsHex(Identity.ChecksumData);

        public Material Material => m_Material;
        public Texture2D Texture => m_Texture;
        // NOTE: Returns a copy of the identity, obtainer cannot modify the original
        internal pSprayIdentityInfo Identity => m_Identity;
        internal byte[] TextureData => m_TextureData;

        public static Spray FromBytes(byte[] dataBytes) => new(dataBytes, true);
        public static Spray FromFile(string filePath) => new(File.ReadAllBytes(filePath), false);

        public SprayInstance Apply(pApplySpray applyData)
        {
            Debug.Assert(applyData.spray == m_Identity, "");
            SprayInstance instance = Instantiate();
            instance.ApplyFromPacket(applyData);
            return instance;
        }

        private Spray(byte[] dataBytes, bool shouldCache = false)
        {
            if (dataBytes.Length >= 4194304)
                throw new NotSupportedException("Spray exceeds 4MB Limit! Please purchase Discord Nitro to exceed this limit");
            // Copy the passed data into our buffer
            m_TextureData = new byte[dataBytes.Length];
            dataBytes.CopyTo(m_TextureData, 0);

            // Build the network identity of our spray
            m_Identity = BuildIdentity(m_TextureData, 0, m_TextureData.Length);

            // Build the texture in il2cpp domain
            m_Texture = new(Constants.SPRAY_WIDTH, Constants.SPRAY_HEIGHT);
            m_Texture.filterMode = FilterMode.Point;
            m_Texture.LoadImage(m_TextureData);

            // Create decal material
            m_Material = new(s_DecalShader);
            m_Material.mainTexture = m_Texture;
            if (shouldCache)
                Resources.Cache.CacheSpray(this);
        }

        private SprayInstance Instantiate() => new(this);
        private pSprayIdentityInfo BuildIdentity(byte[] data, int offset, int length)
        {
            return new()
            {
                ChecksumData = Cache.ChecksumBytes(data, offset, length) 
            };
        }

        private readonly byte[] m_TextureData;
        private readonly Material m_Material;
        private readonly Texture2D m_Texture;
        // How the spray is identified across the network
        private readonly pSprayIdentityInfo m_Identity;

        private static CellSoundPlayer s_SoundPlayer = null;
        // NOTE: Might not be good to obtain on cctor, move to deferred if issues arise
        private static readonly Shader s_DecalShader = Shader.Find("Cell/Decal/DecalDeferredBlend");
    }
}
