using UnityEngine;

namespace RayTracer
{
    [CreateAssetMenu]
    public class QualitySettings : ScriptableObject
    {
        [UnityEngine.Range(0, 4)]
        public int numRayBounces = 2;

        [UnityEngine.Range(0.0f, 0.075f)] 
        public float energyLostPerUnit = 0.005f;

        [UnityEngine.Range(0.0f, 1.0f)]
        public float energyLostPerRoughnessOnReflect = 0.8f;

//        [UnityEngine.Range(0.0f, 2.0f)]
//        public float energyLostPerOpacityOnRefract = 1.0f;
        
        public Color skyColor = new Color(43/255f, 58/255f, 72/255f, 1.0f);
        public Color skyDarkColor = new Color(32/255f, 43/255f, 51/255f, 1.0f);
        public Color ambientLight = Color.black;
    }
}