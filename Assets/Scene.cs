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
                Position = new Vector3(5.0f, 5.0f, 5.0f),
                Radius = 5f,
                Material = new RayMaterial
                {
                    Color = Color.red,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere1);
            
            var sphere2 = new Sphere
            {
                Position = new Vector3(2.0f, 0.0f, 15.0f),
                Radius = 5f,
                Material = new RayMaterial
                {
                    Color = Color.yellow,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere2);
            
            var sphere3 = new Sphere
            {
                Position = new Vector3(-8.0f, -2.0f, 16.0f),
                Radius = 4f,
                Material = new RayMaterial
                {
                    Color = Color.blue,
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere3);
            
            
            

            var light1 = new Sphere
            {
                Position = new Vector3(-20.0f, 50.0f, -20.0f),
                Radius = 1f,
                Material = new RayMaterial
                {
                    Color = Color.white,
                    Opacity = 1f
                }
            };
            Lights.Add(light1);
            
            AllColliders.AddRange(Spheres);
            AllColliders.AddRange(Lights);
        }
    }
}