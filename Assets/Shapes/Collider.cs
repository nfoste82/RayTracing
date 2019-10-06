using UnityEngine;
using RayTracer;

namespace Shapes
{
    public abstract class Collider
    {
        public Vector3 Position;

        public RayMaterial Material;

        public abstract float GetBoundingRadius();

        public abstract float Intersect(
            Vector3 origin,
            Vector3 rayDir);

        public Vector3 GetNormalAtPoint(Vector3 point)
        {
            var diff = (point - Position);
            diff.Normalize();

            return diff;
        }
    }
}