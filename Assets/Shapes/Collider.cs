using UnityEngine;
using RayTracer;
using Ray = RayTracer.Ray;

namespace Shapes
{
    public abstract class Collider
    {
        public Vector3 Position;

        public RayMaterial Material;

        public abstract float GetBoundingRadius();

        public abstract float Intersect(Vector3 origin, Vector3 direction);

        public abstract void Refract(Ray ray, Intersection intersection);

        public Vector3 GetNormalAtPoint(Vector3 point)
        {
            var diff = (point - Position);
            diff.Normalize();

            return diff;
        }
    }
}