using UnityEngine;

namespace RayTracer
{
    public class Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public Color Color;
        public float Energy;
        
        public Vector3 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }
        
        public Vector3 GetPoint(double distance)
        {
            return Origin + Direction * (float)distance;
        }
    }
}