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

            var refraction = (targetRefraction - sourceRefraction) / maxRefractionDiff;
            var reflectAmount = refraction * maxReflect;

            // reflect amount
            // ------------------
            // 0 no refraction
            // -2 (-90 degrees)
            // 2 (90 degrees)
            
            reflectAmount = reflectAmount * Vector3.Dot(targetNormal, sourceDirection);
            return new Vector3(reflectAmount * targetNormal.x + sourceDirection.x, 
                reflectAmount * targetNormal.y + sourceDirection.y, 
                reflectAmount * targetNormal.z + sourceDirection.z);
        }

        public static Vector3 GetClosestPointOnLineSegment(Vector3 LinePointStart, Vector3 LinePointEnd, Vector3 testPoint)
        {
            Vector3 lineDiff = LinePointEnd - LinePointStart;
            float lineSegSqrLength = lineDiff.sqrMagnitude;
 
            Vector3 lineToPoint = testPoint - LinePointStart;
            float dotProduct = Vector3.Dot(lineDiff, lineToPoint);
 
            float percentageAlongLine = dotProduct / lineSegSqrLength;
 
            if (percentageAlongLine < 0.0f || percentageAlongLine > 1.0f)
            {
                // Point isn't within the line segment
                return Vector3.zero;
            }
 
            return LinePointStart + (percentageAlongLine * (LinePointEnd - LinePointStart));
        }
    }
}