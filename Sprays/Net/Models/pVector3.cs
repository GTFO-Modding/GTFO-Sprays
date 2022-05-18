using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Sprays.Net.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct pVector3
    {
        public pVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x;
        public float y;
        public float z;

        public static implicit operator Vector3(pVector3 vector) => new(vector.x, vector.y, vector.z);
        public static implicit operator pVector3(Vector3 vector) => new(vector.x, vector.y, vector.z);
    }
}
