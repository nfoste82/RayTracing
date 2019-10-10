using UnityEngine;
using System;

namespace RayTracer
{
    public static class ColorExtensions
    {
        public static Color Combine(Color a, Color b)
        {
            a.r = Math.Max(a.r, b.r);
            a.g = Math.Max(a.g, b.g);
            a.b = Math.Max(a.b, b.b);

            return a;
        }
    }
}