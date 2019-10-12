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
                Position = new Vector3(7.0f, -15.0f, 12.0f),
                Radius = 4f,
                Material = new RayMaterial
                {
                    Color = Color.blue,
                    Roughness = 1f,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere1);
            
            var sphere2 = new Sphere
            {
                Position = new Vector3(2.0f, -18.0f, 3.0f),
                Radius = 2f,
                Material = new RayMaterial
                {
                    Color = Color.green, //new Color(0.6f, 0.5f, 0.9f, 1.0f),
                    Roughness = 0.1f,
                    Opacity = 0.1f,
                    RefractionIndex = 1.7f
                }
            };
            Spheres.Add(sphere2);
            
            var sphere3 = new Sphere
            {
                Position = new Vector3(-3.0f, -16.0f, 1.0f),
                Radius = 1.5f,
                Material = new RayMaterial
                {
                    Color = Color.red,
                    Roughness = 0.1f,
                    Opacity = 0.2f,
                    RefractionIndex = 1.35f
                }
            };
            Spheres.Add(sphere3);
            
            var sphere4 = new Sphere
            {
                Position = new Vector3(-7.0f, -17.0f, 8.0f),
                Radius = 3f,
                Material = new RayMaterial
                {
                    Color = Color.yellow,
                    Roughness = 0.0f,
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
                    Roughness = 0.5f,
                    Opacity = 1f,
                    CheckeredTexture = true,
                }
            };
            Spheres.Add(sphere5);
            
            

            var light1 = new Sphere
            {
                Position = new Vector3(0.0f, -10.0f, 10.0f),
                Radius = 1f,
                Material = new RayMaterial
                {
                    Color = new Color(1.0f, 1.0f, 0.7f, 1.0f),
                    Opacity = 1f,
                    Emissive = 1f,
                }
            };
            Lights.Add(light1);
            
            var light2 = new Sphere
            {
                Position = new Vector3(6.0f, -18.0f, -3.0f),
                Radius = 1f,
                Material = new RayMaterial
                {
                    Color = new Color(1.0f, 1.0f, 0.7f, 1.0f),//new Color(0.5f, 1f, 1.0f, 1.0f),
                    Opacity = 1f,
                    Emissive = 1.2f
                }
            };
            Lights.Add(light2);
            
            AllColliders.AddRange(Spheres);
            AllColliders.AddRange(Lights);
        }
    }
}