using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace RayTracer
{
    public class Scene
    {
        public readonly List<Sphere> Spheres = new List<Sphere>();
        
        public readonly List<Sphere> Lights = new List<Sphere>();

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
            
            
            

            var light1 = new Sphere
            {
                Position = new Vector3(0.0f, 50.0f, 0.0f),
                Radius = 5f,
                Material = new RayMaterial
                {
                    Color = Color.white,
                    Opacity = 1f
                }
            };
            Lights.Add(light1);
        }
    }
}