using UnityEngine;
using RayTracer;

namespace Shapes
{
    public class Collider
    {
        public Vector3 Position { get; set; }
        
        public RayMaterial Material { get; set; }

        public static (bool intersected, double hitDistance) Intersect(
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
            if (collider is Sphere sphere)
            {
                return (point - collider.Position).normalized;
            }
            
            return Vector3.zero;
        }
    }
}