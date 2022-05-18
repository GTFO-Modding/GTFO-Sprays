using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprays.Net.Models
{
    // NOTE: Describes the indentity information of a spray
    // Keep in sync with Spray checksum generation
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pSprayIdentityInfo
    {
        public Spray SprayObject
        {
            get
            {
                // Can't use LINQ because of `this`
                foreach (Spray spray in RuntimeLookup.Sprays)
                    if (spray.Identity == this) return spray;

                // RuntimeLookup.Sprays [currently] contains the LocalSpray, so this call isn't necessary
                // but we'll do it for good measure
                return LocalSprayObject;
            }
        }

        public Spray LocalSprayObject
        {
            get
            {
                // Can't use LINQ because of `this`
                foreach (Spray spray in RuntimeLookup.LocalSprays)
                    if (spray.Identity == this) return spray;

                return null;
            }
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 / 8)]
        public byte[] ChecksumData;

        public static bool operator !=(pSprayIdentityInfo lhs, pSprayIdentityInfo rhs) => !(lhs == rhs);
        public static bool operator ==(pSprayIdentityInfo lhs, pSprayIdentityInfo rhs)
        {
            if (lhs.ChecksumData == null && rhs.ChecksumData == null) return true;
            if (lhs.ChecksumData == null || rhs.ChecksumData == null) return false;

            if (lhs.ChecksumData.Length != rhs.ChecksumData.Length) return false;

            for (byte idx = 0; idx < lhs.ChecksumData.Length; ++idx)
            {
                if (lhs.ChecksumData[idx] != rhs.ChecksumData[idx])
                    return false;
            }
            return true;
        }
    }
}
