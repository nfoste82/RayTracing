using UnityEngine;

namespace RayTracer
{
    public static class Vector3Extensions
    {
        public static float NormalizeReturnMag(ref Vector3 vect)
        {
            var mag = vect.magnitude;
            if (mag > 9.99999974737875E-06)
            {
                vect /= mag;
            }
            else
            {
                vect = Vector3.zero;
            }

            return mag;
        }
    }
}