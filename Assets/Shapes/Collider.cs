using UnityEngine;
using RayTracer;

namespace Shapes
{
    public abstract class Collider
    {
        public Vector3 Position;

        public RayMaterial Material;

        public abstract float GetBoundingRadius();

        public static float Intersect(
            Vector3 origin, 
            Collider collider,
            Vector3 rayDir)
        {
            if (collider is Sphere sphere)
            {
                return Sphere.Intersect(origin, sphere, rayDir);
            }

            return default;
        }

        public static Vector3 GetNormalAtPoint(Vector3 point, Collider collider)
        {
            if (collider is Sphere)
            {
                var diff = (point - collider.Position);
                diff.Normalize();

                return diff;
            }
            
            return Vector3.zero;
        }
    }
}