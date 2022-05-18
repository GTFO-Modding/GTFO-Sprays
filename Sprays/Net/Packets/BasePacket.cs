using GTFO.API;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprays.Net.Packets
{
    internal abstract class BasePacket<TSelf, TPayload>
        where TSelf : BasePacket<TSelf, TPayload>
        where TPayload : struct
    {
        public static TSelf Instance { get; private set; }

        public abstract void OnReceived(SNet_Player sender, TPayload payload);
        // Allows the implementor to manipulate the packet being sent before being
        // passed to the internal Networking layer
        protected virtual void ManipulateSendData(ref TPayload payload) { }

        #region Send Methods
        public void Send(TPayload payload)
        {
            lock (s_ActionLock)
            {
                ManipulateSendData(ref payload);
                NetworkAPI.InvokeEvent(s_EventName, payload);
            }
        }
        public void Send(TPayload payload, SNet_Player target)
        {
            lock (s_ActionLock)
            {
                ManipulateSendData(ref payload);
                NetworkAPI.InvokeEvent(s_EventName, payload, target);
            }
        }
        public void Send(TPayload payload, List<SNet_Player> targets)
        {
            lock (s_ActionLock)
            {
                ManipulateSendData(ref payload);
                NetworkAPI.InvokeEvent(s_EventName, payload, targets);
            }
        }
        #endregion

        protected void RegisterImpl(string eventName)
        {
            // Ensure we haven't registered ourselves
            if (Instance != null) return;

            // Acquire a lock
            lock (s_ActionLock)
            {
                // Ensure we haven't registered ourselves while waiting for the lock
                if (Instance != null) return;

                // We're the first instance to register,
                // set static data and register ourselves in the NetworkAPI

                s_EventName = eventName;
                NetworkAPI.RegisterEvent<TPayload>(s_EventName, (senderId, payload) => {
                    if (!SNet.TryGetPlayer(senderId, out var sender))
                    {
                        L.Error($"{GetType()}: Failed to obtain SNet_Player from senderId");
                        return;
                    }
                    OnReceived(sender, payload);
                });

                Instance = (TSelf)this;
            }
        }

        // Needed for Send
        private static string s_EventName = null;

        private static readonly object s_ActionLock = new();
    }
}
