using UnityEngine;

namespace RayTracer
{
    public class RayMaterial
    {
        public Color Color;
        public float Roughness;    // 0f - 1f
        public float Emissive;     // 0f - 1f (anything greater than zero emits light)
        
        public float Opacity;      // 0f - 1f
        public float RefractionIndex;    // Only needed if opacity is < 1.0f

        public bool CheckeredTexture;
        
    }
}