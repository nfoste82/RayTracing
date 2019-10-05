using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace RayTracer
{
    public class Scene
    {
        public readonly List<Sphere> Spheres = new List<Sphere>();

        public void CreateTestScene()
        {
            var sphere1 = new Sphere
            {
                Position = new Vector3(5.0f, 5.0f, 5.0f),
                Radius = 5f,
                Material = new RayMaterial
                {
                    Color = new Color(1f, 0f, 0f, 1f),
                    Opacity = 1f
                }
            };
            Spheres.Add(sphere1);
        }
    }
}