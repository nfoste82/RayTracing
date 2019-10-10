using System.Collections.Generic;
using Shapes;
using UnityEngine;

using Collider = Shapes.Collider;

namespace RayTracer
{
    public class Scene
    {
        public readonly List<Collider> Spheres = new List<Collider>();
        public readonly List<Collider> Lights = new List<Collider>();
        public readonly List<Collider> AllColliders = new List<Collider>();

        public void CreateTestScene()
        {
            var sphere1 = new Sphere
            {
                Position = new Vector3(15.0f, 0.0f, 45.0f),
                Radius = 12f,
                Material = new RayMaterial
                {
                    Color = Color.blue,
                    Roughness = 0.1f,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere1);
            
            var sphere2 = new Sphere
            {
                Position = new Vector3(11.0f, -10.0f, 10.0f),
                Radius = 6,
                Material = new RayMaterial
                {
                    Color = Color.green,
                    Roughness = 0.5f,
                    Opacity = 0.5f,
                    RefractionIndex = 1.4f
                }
            };
            Spheres.Add(sphere2);
            
            var sphere3 = new Sphere
            {
                Position = new Vector3(-2.0f, -10.0f, 11.0f),
                Radius = 4f,
                Material = new RayMaterial
                {
                    Color = Color.red,
                    Roughness = 0.15f,
                    Opacity = 0.35f,
                    RefractionIndex = 1.35f
                }
            };
            Spheres.Add(sphere3);
            
            var sphere4 = new Sphere
            {
                Position = new Vector3(-14.0f, -10.0f, 23.0f),
                Radius = 9f,
                Material = new RayMaterial
                {
                    Color = Color.yellow,
                    Roughness = 0.6f,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere4);
            
            var sphere5 = new Sphere
            {
                Position = new Vector3(0.0f, -10020.0f, 0.0f),
                Radius = 10000f,
                Material = new RayMaterial
                {
                    Color = Color.white,
                    Roughness = 0.8f,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere5);
            
            

            var light1 = new Sphere
            {
                Position = new Vector3(-20.0f, 50.0f, -20.0f),
                Radius = 1f,
                Material = new RayMaterial
                {
                    Color = new Color(1.0f, 1.0f, 0.7f, 1.0f),
                    Opacity = 1f
                }
            };
            Lights.Add(light1);
            
            // Multiple lights work. Uncomment to try
//            var light2 = new Sphere
//            {
//                Position = new Vector3(20.0f, 30.0f, 20.0f),
//                Radius = 1f,
//                Material = new RayMaterial
//                {
//                    Color = new Color(0.8f, 0.9f, 1.0f, 1.0f),
//                    Opacity = 1f
//                }
//            };
//            Lights.Add(light2);
            
            AllColliders.AddRange(Spheres);
            AllColliders.AddRange(Lights);
        }
    }
}