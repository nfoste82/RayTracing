using UnityEngine;

namespace RayTracer
{
    public class RayMaterial
    {
        public Color Color;
        public float Opacity;
        public float RefractionIndex;    // Only needed if opacity is < 1.0f
        public float Roughness;

        public bool CheckeredTexture;
        public bool LightEmitter;
    }
}