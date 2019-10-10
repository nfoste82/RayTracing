using UnityEngine;
using Collider = Shapes.Collider;

namespace RayTracer
{
    public struct Intersection
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Collider Collider;
    }
}