using System;
using RayTracer;
using UnityEngine;

namespace Shapes
{
    public class Sphere
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

        public RayMaterial Material { get; set; }

        public static (bool intersected, double hitDistance) Intersect(Vector3 origin, Sphere sphere, Vector3 rayDir, float? maxDistance = null)
        {
            var diffToSphere = origin - sphere.Position; 
            var b = Vector3.Dot(diffToSphere, rayDir); 
            var c = diffToSphere.sqrMagnitude - sphere.Radius * sphere.Radius; 

            // Exit if ray’s origin outside sphere (c > 0) and ray is pointing away from sphere (b > 0) 
            if (c > 0.0f && b > 0.0f)
            {
                return (false, -1.0);
            }

            var discriminant = (b * b) - c; 

            // A negative discriminant corresponds to ray missing sphere 
            if (discriminant < 0.0f)
            {
                return (false, -1.0);
            } 

            // Ray now found to intersect sphere, compute smallest t value of intersection
            var hitDistance = -b - Math.Sqrt(discriminant);

            // If hit distance is negative, ray started inside sphere so clamp it to zero
            if (hitDistance < 0.0)
            {
                hitDistance = 0.0;
            }

            return (true, hitDistance);
        }
    }
}