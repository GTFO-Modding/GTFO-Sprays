using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Decals;
using GTFO.API;
using BepInEx;
using System.IO;
using Player;
using System.Runtime.InteropServices;

namespace Sprays
{
    public class NetworkedSprays
    {
        public static void Setup()
        {
            Current = new();
            var spraysPath = Path.Combine(Paths.ConfigPath, "sprays");
            if (!Directory.Exists(spraysPath)) Directory.CreateDirectory(spraysPath);

            var sprayImagePath = Directory.GetFiles(spraysPath)[0];

            var sprayData = File.ReadAllBytes(sprayImagePath);
            if (sprayData.Length >= LIMIT_FILESIZE)
            {
                L.Error("Spray exceeds 4MB Limit! Please purchase Discord Nitro to exceed this limit");
                return;
            }

            s_RawTextureData = new byte[(int)Math.Ceiling(sprayData.Length / (decimal)IMAGEPACKET_CHUNKSIZE) * IMAGEPACKET_CHUNKSIZE];
            Array.Copy(sprayData, s_RawTextureData, sprayData.Length);

            s_LocalSpray = new Texture2D(SPRAY_WIDTH, SPRAY_HEIGHT);
            s_LocalSpray.filterMode = FilterMode.Point;
            s_LocalSpray.LoadImage(s_RawTextureData);
            Current.m_PlayerSprays[PlayerManager.GetLocalPlayerSlotIndex()] = s_LocalSpray;

            if (!s_IsSetup)
            {
                s_IsSetup = true;

                Current.m_InputHandler = new GameObject("SprayInputHandler").AddComponent<SprayInputHandler>();
                Current.m_SpraySoundPlayer = new();
            }

        }
        public static void OnReceiveSprayDataRequest(ulong x, byte y)
        {
            L.Debug("Recieved spray data request from host. Sending our own spray data and requesting other clients to send theirs");
            Setup();
            Current.PostLocalSprayData();

            NetworkAPI.InvokeEvent("ClientReplySprayData", 0);
        }
        public static void OnReceivePostSprayRequest(ulong x, byte y)
        {
            L.Debug("Received spray data request from client, sending our spray data to them");
            Current.PostLocalSprayData();
        }
        public static void OnReceiveClearSprayData(ulong x, int slot)
        {
            L.Debug($"Clearing spray data for player {slot}");
            Current.m_PlayerSprays[slot] = null;

            if (Current.m_PlayerSprayDecals[slot] != null)
            {
                Current.m_PlayerSprayDecals[slot].Shown(false);
                Current.m_PlayerSprayDecals[slot] = null;
            }
        }
        public static void OnReceiveSprayData(ulong x, pSprayData sprayData)
        {
            Buffer.BlockCopy(sprayData.RawTextureData, 0, Current.m_PlayerImageData[sprayData.PlayerSlot], sprayData.RawDataOffset, IMAGEPACKET_CHUNKSIZE);
            if (!sprayData.FinalChunk) return;

            var sprayTex = new Texture2D(SPRAY_WIDTH, SPRAY_HEIGHT);
            sprayTex.filterMode = FilterMode.Point;

            sprayTex.LoadImage(Current.m_PlayerImageData[sprayData.PlayerSlot]);
            Current.m_PlayerSprays[sprayData.PlayerSlot] = sprayTex;
        }
        public static void OnReceiveApplySpray(ulong x, pReceiveSprayData data)
        {

            var player = PlayerManager.Current.GetPlayerAgentInSlot(data.Slot);

            if (Current.m_PlayerSprayDecals[data.Slot] == null)
            {
                var sprayDecalShader = Shader.Find("Cell/Decal/DecalDeferredBlend");
                var sprayDecalMat = new Material(sprayDecalShader);
                var sprayGO = new GameObject("SprayGO");
                sprayGO.transform.localScale = Vector3.one * 1.5f;

                var sprayDecal = sprayGO.AddComponent<Decal>();
                sprayDecal.m_decalMaterial = sprayDecalMat;
                sprayDecal.m_decalType = Decal.DecalType.Generic;

                Current.m_PlayerSprayDecals[data.Slot] = sprayDecal;
            }

            Current.m_PlayerSprayDecals[data.Slot].m_decalMaterial.mainTexture = Current.m_PlayerSprays[data.Slot];

            Current.m_PlayerSprayDecals[data.Slot].transform.position = new(data.PosX, data.PosY, data.PosZ);
            Current.m_PlayerSprayDecals[data.Slot].transform.localPosition += new Vector3(0, 0, -0.05f);
            Current.m_PlayerSprayDecals[data.Slot].transform.rotation = Quaternion.LookRotation(new(data.RotX, data.RotY, data.RotZ));
            Current.m_PlayerSprayDecals[data.Slot].m_isRegistered = false;
            Current.m_PlayerSprayDecals[data.Slot].ResetMatrix();
            Current.m_PlayerSprayDecals[data.Slot].Shown(true);

            if (Current.m_SpraySoundPlayer == null) Current.m_SpraySoundPlayer = new();
            Current.m_SpraySoundPlayer.Post(AK.EVENTS.MEDSPRAYLOOPEND, new Vector3(data.PosX, data.PosY, data.PosZ));
        }



        public void PostLocalSprayData()
        {
            L.Debug("Sending spray image data to all connected players");

            var slot = PlayerManager.GetLocalPlayerSlotIndex();
            var chunkCount = Math.Ceiling(s_RawTextureData.Length / (decimal)IMAGEPACKET_CHUNKSIZE);
            var chunk = new byte[IMAGEPACKET_CHUNKSIZE];
            
            pSprayData sprayDataChunk;

            for (var i = 0; i < chunkCount; i++)
            {
                Array.Copy(s_RawTextureData, i * IMAGEPACKET_CHUNKSIZE, chunk, 0, IMAGEPACKET_CHUNKSIZE);

                sprayDataChunk = new pSprayData()
                {
                    PlayerSlot = slot,
                    RawDataOffset = i * IMAGEPACKET_CHUNKSIZE,
                    RawTextureData = chunk,
                    FinalChunk = (i + 1 == chunkCount)
                };

                NetworkAPI.InvokeEvent("PostSprayData", sprayDataChunk);
            }

            m_PlayerSprays[slot] = s_LocalSpray;
        }

        public void SyncApplySpray()
        {
            var slot = PlayerManager.GetLocalPlayerSlotIndex();
            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            var rayHit = localPlayer.FPSCamera.m_camRayHit;

            var data = new pReceiveSprayData()
            {
                PosX = rayHit.point.x,
                PosY = rayHit.point.y,
                PosZ = rayHit.point.z,

                RotX = rayHit.normal.x,
                RotY = rayHit.normal.y,
                RotZ = rayHit.normal.z,

                Slot = slot
            };

            NetworkAPI.InvokeEvent("ApplyPlayerSpray", data);
            OnReceiveApplySpray(0, data);
        }



        public const int LIMIT_FILESIZE = 4194304;
        public const int IMAGEPACKET_CHUNKSIZE = 512;
        public const int SPRAY_WIDTH = 2048;
        public const int SPRAY_HEIGHT = 2048;

        public static NetworkedSprays Current;
        public static bool s_IsSetup = false;
        public static Texture2D s_LocalSpray;
        public static byte[] s_RawTextureData;

        public byte[][] m_PlayerImageData = new byte[][] { new byte[LIMIT_FILESIZE], new byte[LIMIT_FILESIZE], new byte[LIMIT_FILESIZE], new byte[LIMIT_FILESIZE] }; // >:(
        public Texture2D[] m_PlayerSprays = new Texture2D[4];
        public Decal[] m_PlayerSprayDecals = new Decal[4];
        public CellSoundPlayer m_SpraySoundPlayer;
        public SprayInputHandler m_InputHandler;

        public struct pSprayData
        {
            public int PlayerSlot;
            public int RawDataOffset;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IMAGEPACKET_CHUNKSIZE)]
            public byte[] RawTextureData;

            public bool FinalChunk;
        }

        public struct pReceiveSprayData
        {
            public float PosX;
            public float PosY;
            public float PosZ;

            public float RotX;
            public float RotY;
            public float RotZ;

            public int Slot;
        }
    }
}
