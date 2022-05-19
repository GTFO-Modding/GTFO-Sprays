using Player;
using Sprays.Net.Packets;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sprays
{
    public class SprayInputHandler : MonoBehaviour
    {
        public SprayInputHandler(IntPtr value) : base(value) { }

        public void Update()
        {
            if (!Input.GetKeyDown(EntryPoint.SprayKey.Value)) return;
            if (!PlayerManager.HasLocalPlayerAgent()) return;
            if (FocusStateManager.CurrentState != eFocusState.FPS) return;

            if (m_PickedSpray == null || m_ReloadSpray == true)
            {
                m_ReloadSpray = false;
                m_PickedSpray = RuntimeLookup.LocalSprays[m_SprayIndex];
            }

            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            var rayHit = localPlayer.FPSCamera.m_camRayHit;

            var packet = new pApplySpray
            {
                spray = m_PickedSpray.Identity,
                position = rayHit.point,
                forward = rayHit.normal,
            };
            ApplySpray.Instance.Send(packet);
            m_PickedSpray.Apply(packet);
        }

        public static SprayInputHandler Current;

        private Spray m_PickedSpray = null;
        public bool m_ReloadSpray = false;
        public int m_SprayIndex = 0;
    }
}
