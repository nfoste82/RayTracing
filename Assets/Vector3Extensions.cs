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

        public static Vector3 Refract(
            this Vector3 sourceDirection, 
            float sourceRefraction, 
            float targetRefraction,
            Vector3 targetNormal)
        {
            const float maxRefractionDiff = 4;
            const float maxReflect = 2;

            float refraction = (targetRefraction - sourceRefraction) / maxRefractionDiff;
            var reflectAmount = refraction * maxReflect;
            
            // target - source (2 - 1)
            
            
            // reflect amount
            // 0 no refraction
            // -2 (-90 degrees)
            // 2 (90 degrees)
            
            float num = -reflectAmount * Vector3.Dot(targetNormal, sourceDirection);
            return new Vector3(num * targetNormal.x + sourceDirection.x, num * targetNormal.y + sourceDirection.y, num * targetNormal.z + sourceDirection.z);
        }
    }
}