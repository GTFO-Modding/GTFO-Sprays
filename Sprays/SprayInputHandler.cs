using Player;
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
            if (NetworkedSprays.s_LocalSpray == null) return;
            if (!PlayerManager.HasLocalPlayerAgent()) return;
            if (FocusStateManager.CurrentState != eFocusState.FPS) return;

            NetworkedSprays.Current.SyncApplySpray();
        }
    }
}
