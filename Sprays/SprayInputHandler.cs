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

            if(m_PickedSpray == null) m_PickedSpray = RuntimeLookup.LocalSprays[LOCAL_SPRAY_IDX];

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

        // TODO: Remove const and add UI for picking sprays
        private Spray m_PickedSpray = null;
        private const int LOCAL_SPRAY_IDX = 0;
    }
}
