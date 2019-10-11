using UnityEngine;

namespace RayTracer
{
    [CreateAssetMenu]
    public class QualitySettings : ScriptableObject
    {
        [Range(0, 4)]
        public int numRayBounces = 3;
    }
}