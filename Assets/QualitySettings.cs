using UnityEngine;

namespace RayTracer
{
    [CreateAssetMenu]
    public class QualitySettings : ScriptableObject
    {
        [Range(0, 3)]
        public int numRayBounces = 1;
    }
}