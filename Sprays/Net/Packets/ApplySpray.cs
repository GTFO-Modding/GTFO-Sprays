using GTFO.API;
using SNetwork;
using Sprays.Net.Models;
using System.Runtime.InteropServices;

namespace Sprays.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pApplySpray
    {
        public pSprayIdentityInfo spray;
        public pVector3 position;
        public pVector3 forward;
    }
    internal class ApplySpray : BasePacket<ApplySpray, pApplySpray>
    {
        internal const string EVENT_NAME = $"SpraysNet_{nameof(ApplySpray)}";
        public static void Register() => new ApplySpray().RegisterImpl(EVENT_NAME);

        public override void OnReceived(SNet_Player sender, pApplySpray payload)
        {
            L.Verbose($"{sender.NickName} ({sender.Lookup}) wants to ApplySpray");
            Spray spray = payload.spray.SprayObject;
            if(spray == null)
            {
                L.Error($"{sender.NickName} ({sender.Lookup}) tried to apply a spray that doesn't exist locally.");
                return;
            }

            spray.Apply(payload);
        }
    }
}
