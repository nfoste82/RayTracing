using UnityEngine;

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

        public override float Intersect(Vector3 origin, Vector3 rayDir)
        {
            var diffToSphere = Position - origin;
            var b = Vector3.Dot(diffToSphere, rayDir);

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
    }
}