using RayTracer;
using UnityEngine;

using Ray = RayTracer.Ray;

namespace Shapes
{
    public class Sphere : Collider
    {
        public override float GetBoundingRadius()
        {
            return _radius;
        }
        
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                RadiusSquared = _radius * _radius;
            }
        }
        private float _radius;

        private float RadiusSquared;

        public override float Intersect(Vector3 origin, Vector3 direction)
        {
            var diffToSphere = Position - origin;
            var b = Vector3.Dot(diffToSphere, direction);

            // ray is pointing away from sphere (b < 0)
            if (b < 0f)
            {
                return -1.0f;
            }
            
            var c = diffToSphere.sqrMagnitude - RadiusSquared;

            var discriminant = (b * b) - c; 

            // A negative discriminant corresponds to ray missing sphere 
            if (discriminant < 0.0f)
            {
                return -1.0f;
            } 

            // Ray now found to intersect sphere, compute smallest t value of intersection
            var hitDistance = b - Mathf.Sqrt(discriminant) - 0.001f;

            // If hit distance is negative, ray started inside sphere so clamp it to zero
            if (hitDistance < 0.0f)
            {
                hitDistance = 0.0f;
            }

            return hitDistance;
        }

        public override void Refract(Ray ray, Intersection intersection)
        {
            ray.Origin = intersection.Position;
                        
            // Refract when we hit the transparent object
            var colliderRefIndex = Material.RefractionIndex;
            ray.Direction = ray.Direction.Refract(1.0f, colliderRefIndex, intersection.Normal);
                        
            // Calculate exit point of sphere
            var halfway = Vector3Extensions.GetClosestPointOnLineSegment(ray.Origin, ray.Origin + 2 * _radius * ray.Direction,
                Position);
            var exitPoint = ((halfway - ray.Origin) * 2f) + ray.Origin;
            var exitPointNormal = (exitPoint - Position);
            exitPointNormal.Normalize();

            // Now refract again since we've left the sphere
            ray.Direction = ray.Direction.Refract(colliderRefIndex, 1.0f, exitPointNormal);
            ray.Origin = exitPoint;
        }
    }
}